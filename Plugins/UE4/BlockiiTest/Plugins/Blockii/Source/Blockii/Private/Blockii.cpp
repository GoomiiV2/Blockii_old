// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "Blockii.h"
#include "BlockiiStyle.h"
#include "BlockiiCommands.h"
#include "Misc/MessageDialog.h"
#include "Framework/MultiBox/MultiBoxBuilder.h"
#include "ISettingsModule.h"
#include "ICollectionManager.h"
#include "BlockiiSettings.h"
#include "AssetTagsSubsystem.h"
#include "Materials/Material.h"
#include "Materials/MaterialInstance.h"
#include "ImageWriteBlueprintLibrary.h"
#include "Kismet/KismetRenderingLibrary.h"
#include "ImageUtils.h"
#include "Misc/FileHelper.h"
#include "RHICommandList.h"
#include "Factories/ReimportTextureFactory.h"
#include "Misc/ScopedSlowTask.h"
#include "ICollectionManager.h"
#include "CollectionManagerModule.h"

#include "LevelEditor.h"
#include <Editor\UnrealEd\Classes\MaterialGraph\MaterialGraph.h>
#include <Runtime\ImageWriteQueue\Public\ImageWriteBlueprintLibrary.h>

DEFINE_LOG_CATEGORY(Blockii)

static const FName BlockiiTabName("Blockii");

#define LOCTEXT_NAMESPACE "FBlockiiModule"

void FBlockiiModule::StartupModule()
{
	// This code will execute after your module is loaded into memory; the exact timing is specified in the .uplugin file per-module
	
	FBlockiiStyle::Initialize();
	FBlockiiStyle::ReloadTextures();
	RegisterSettings();

	FBlockiiCommands::Register();
	
	PluginCommands = MakeShareable(new FUICommandList);

	PluginCommands->MapAction(
		FBlockiiCommands::Get().PluginAction,
		FExecuteAction::CreateRaw(this, &FBlockiiModule::PluginButtonClicked),
		FCanExecuteAction());
		
	FLevelEditorModule& LevelEditorModule = FModuleManager::LoadModuleChecked<FLevelEditorModule>("LevelEditor");
	
	{
		TSharedPtr<FExtender> MenuExtender = MakeShareable(new FExtender());
		MenuExtender->AddMenuExtension("WindowLayout", EExtensionHook::After, PluginCommands, FMenuExtensionDelegate::CreateRaw(this, &FBlockiiModule::AddMenuExtension));

		LevelEditorModule.GetMenuExtensibilityManager()->AddExtender(MenuExtender);
	}
	
	{
		TSharedPtr<FExtender> ToolbarExtender = MakeShareable(new FExtender);
		ToolbarExtender->AddToolBarExtension("Settings", EExtensionHook::After, PluginCommands, FToolBarExtensionDelegate::CreateRaw(this, &FBlockiiModule::AddToolbarExtension));
		
		LevelEditorModule.GetToolBarExtensibilityManager()->AddExtender(ToolbarExtender);
	}
}

void FBlockiiModule::ShutdownModule()
{
	// This function may be called during shutdown to clean up your module.  For modules that support dynamic reloading,
	// we call this function before unloading the module.
	FBlockiiStyle::Shutdown();

	FBlockiiCommands::Unregister();
	UnRegisterSettings();
}

void FBlockiiModule::RegisterSettings()
{
	if (ISettingsModule* SettingsModule = FModuleManager::GetModulePtr<ISettingsModule>(FName(TEXT("Settings"))))
	{
		Settings = GetMutableDefault<UBlockiiSettings>();
		SettingsModule->RegisterSettings("Project", "Plugins", "Blockii",
			LOCTEXT("RuntimeSettingsName", "Blockii"),
			LOCTEXT("RuntimeSettingsDescription", "Configure settings for Blockii"),
			Settings
		);
	}
}

void FBlockiiModule::UnRegisterSettings()
{
	if (ISettingsModule* SettingsModule = FModuleManager::GetModulePtr<ISettingsModule>("Settings"))
	{
		SettingsModule->UnregisterSettings("Project", "Plugins", "Blockii");
	}
}

void FBlockiiModule::PluginButtonClicked()
{
	GetMatsInCollection();

	// Put your "OnButtonClicked" stuff here
	/*FText DialogText = FText::Format(
							LOCTEXT("PluginButtonDialogText", "Add code to {0} in {1} to override this button's actions"),
							FText::FromString(TEXT("FBlockiiModule::PluginButtonClicked()")),
							FText::FromString(TEXT("Blockii.cpp"))
					   );
	FMessageDialog::Open(EAppMsgType::Ok, DialogText);*/
}

void FBlockiiModule::AddMenuExtension(FMenuBuilder& Builder)
{
	Builder.AddMenuEntry(FBlockiiCommands::Get().PluginAction);
}

void FBlockiiModule::AddToolbarExtension(FToolBarBuilder& Builder)
{
	Builder.AddToolBarButton(FBlockiiCommands::Get().PluginAction);
}

void FBlockiiModule::GetMatsInCollection()
{
	UAssetTagsSubsystem* AssetTags = GEditor->GetEngineSubsystem<UAssetTagsSubsystem>();

	// Get all the collections to search
	ICollectionManager& CollectionManager = FCollectionManagerModule::GetModule().Get();
	TArray<FName> collectionsToSearch;
	CollectionManager.GetChildCollectionNames(PARENT_COLLECTION_NAME, ECollectionShareType::Type::CST_All, ECollectionShareType::Type::CST_All, collectionsToSearch);
	collectionsToSearch.Append(Settings->MaterialCollectionsToExport);

	int32 numMatsToExport = 0;
	TArray<TTuple<FName, TArray<FAssetData>>> matGroups;
	for (auto collection : collectionsToSearch)
	{
		auto assests = AssetTags->GetAssetsInCollection(collection);
		TArray<FAssetData> assetDatas;
		for (auto asset : assests)
		{
			if (asset.AssetClass == FName(TEXT("MaterialInstanceConstant")))
			{
				UE_LOG(Blockii, Log, TEXT("Collection: %s, class: %s"), *collection.ToString(), *asset.AssetClass.ToString());
				assetDatas.Add(asset);
				numMatsToExport++;
			}
		}

		matGroups.Add(MakeTuple(collection, assetDatas));
	}

	if (matGroups.Num() == 0)
	{
		FMessageDialog::Open(EAppMsgType::Ok, LOCTEXT("NoMatsInCollections", "There where no Material Instances in collections to export./n Add any Material Instance that you want exported to a collection parented under Blockii or add its name to the 'Material Collections To Export' setting under the plugins settings."));
	}

	// Now export them
	FScopedSlowTask exportTexturesTask(numMatsToExport, LOCTEXT("ExportingTextures", "Exporting textures"));
	exportTexturesTask.MakeDialog();
	for (auto matGroup : matGroups)
	{
		UE_LOG(Blockii, Log, TEXT("Exporting %i mats in collection: %s"), matGroup.Value.Num(), *matGroup.Key.ToString());
		for (auto assetData : matGroup.Value)
		{
			exportTexturesTask.EnterProgressFrame();
			ExportMat(matGroup.Key, assetData);
		}
	}
}



void FBlockiiModule::ExportMat(FName CollectionName, FAssetData AssetData)
{
	UMaterialInstance* mat = Cast<UMaterialInstance>(AssetData.FastGetAsset(true));
	if (mat == nullptr) { UE_LOG(Blockii, Warning, TEXT("Couldn't load %s as a material :<"), *AssetData.AssetName.ToString()); return; }

	TArray<FMaterialParameterInfo> MaterialInfo;
	TArray<FGuid> MaterialGuids;
	mat->GetAllTextureParameterInfo(MaterialInfo, MaterialGuids);

	for (auto pramaInfos : MaterialInfo)
	{
		UE_LOG(Blockii, Log, TEXT("Prama Name: %s"), *pramaInfos.Name.ToString());

		// Is this a diffuse texture to export?
		if (Settings->DiffuseMaterialPramaNames.Contains(pramaInfos.Name.ToString()))
		{
			UE_LOG(Blockii, Log, TEXT("Yay got a diffuse texture :> : %s"), *pramaInfos.Name.ToString());

			UTexture* tex;
			mat->GetTextureParameterValue(pramaInfos, tex);

			// TODO: cast proper
			ExportTexture(tex, CollectionName, AssetData.AssetName);
		}
	}

	/*for (auto texPrama : mat->TextureParameterValues)
	{
		UE_LOG(Blockii, Log, TEXT("%s"), *texPrama.ParameterInfo.Name.ToString());
	}*/

	/*UMaterial* mat = (UMaterial*)AssetData.FastGetAsset(true);
	if (mat == nullptr) { UE_LOG(Blockii, Warning, TEXT("Couldn't load %s as a material :<"), *AssetData.AssetName.ToString()); return; }

	UMaterialGraph* matGraph = mat->MaterialGraph;
	if (matGraph != nullptr)
	{
		for (FMaterialInputInfo matInput : matGraph->MaterialInputs)
		{
			UE_LOG(Blockii, Log, TEXT("%s"), *matInput.GetName().ToString());
		}
	}*/

	/*auto baseColor = mat->BaseColor;
	if (baseColor != nullptr)
	{
		UE_LOG(Blockii, Log, TEXT("Base Color Name: %s"), *baseColor.Expression->GetFName().ToString());
	}*/
}

void FBlockiiModule::CopyTextureToArray(UTexture2D* Texture, TArray<FColor>& Array) {
	struct FCopyBufferData {
		UTexture2D* Texture;
		TPromise<void> Promise;
		TArray<FColor> DestBuffer;
	};
	using FCommandDataPtr = TSharedPtr<FCopyBufferData, ESPMode::ThreadSafe>;
	FCommandDataPtr CommandData = MakeShared<FCopyBufferData, ESPMode::ThreadSafe>();
	CommandData->Texture = Texture;
	CommandData->DestBuffer.SetNum(Texture->GetSizeX() * Texture->GetSizeY());

	auto Future = CommandData->Promise.GetFuture();

	ENQUEUE_UNIQUE_RENDER_COMMAND_ONEPARAMETER(
		CopyTextureToArray, FCommandDataPtr, CommandData, CommandData, {
		  auto Texture2DRHI = CommandData->Texture->Resource->TextureRHI->GetTexture2D();
		  uint32 DestPitch{0};
		  uint8 * MappedTextureMemory = (uint8*)RHILockTexture2D(
			  Texture2DRHI, 0, EResourceLockMode::RLM_ReadOnly, DestPitch, false);

		  uint32 SizeX = CommandData->Texture->GetSizeX();
		  uint32 SizeY = CommandData->Texture->GetSizeY();

		  FMemory::Memcpy(CommandData->DestBuffer.GetData(), MappedTextureMemory,
						  SizeX * SizeY * sizeof(FColor));

		  RHIUnlockTexture2D(Texture2DRHI, 0, false);
		  // signal completion of the operation
		  CommandData->Promise.SetValue();
		});

	/*ENQUEUE_RENDER_COMMAND(SceneDrawCompletion)(
		[This](FRHICommandListImmediate& RHICmdList)
		{
			This->ExecuteTask();
		});*/

	// wait until render thread operation completes
	Future.Get();

	Array = std::move(CommandData->DestBuffer);
}

void FBlockiiModule::ExportTexture(UTexture* Tex, FName CollectionName, FName MatName)
{
	const UEnum* enumPtr = FindObject<UEnum>(ANY_PACKAGE, TEXT("EDesiredImageFormat"), true);
	FString imgPath = FPaths::Combine(Settings->GetTextureExportBasePath(), CollectionName.ToString() / MatName.ToString() + TEXT(".") + enumPtr->GetNameByValue((int64)Settings->TextureExportFormat).ToString());
	//UKismetRenderingLibrary::ExportTexture2D(GEditor->GetWorld(), (UTexture2D*)Tex, Settings->GetTextureExportBasePath(), imgPath);

	//UReimportTextureFactory reimportFactory = new UReimportTextureFactory();
	auto compressionSettings                  = Tex->CompressionSettings;
	auto mipsSettings                         = Tex->MipGenSettings;
	auto srgb                                 = Tex->SRGB;
	Tex->CompressionSettings                  = TextureCompressionSettings::TC_VectorDisplacementmap;
	Tex->MipGenSettings                       = TextureMipGenSettings::TMGS_NoMipmaps;
	Tex->SRGB                                 = false;
	Tex->UpdateResource();
	//UReimportTextureFactory::Reimport(Tex);

	//UKismetRenderingLibrary::ExportTexture2D(GEditor->GetWorld(), (UTexture2D*)Tex, Settings->GetTextureExportBasePath(), imgPath);

	FImageWriteOptions exportOptions;
	exportOptions.Format             = Settings->TextureExportFormat;
	exportOptions.CompressionQuality = Settings->TextureExportQuality;

	ENQUEUE_RENDER_COMMAND(ResolvePixelData)(
		[Tex, imgPath, exportOptions](FRHICommandListImmediate& RHICmdList)
		{
			UImageWriteBlueprintLibrary::ExportToDisk(Tex, imgPath, exportOptions);
		});

	//const FColor* FormatedImageData = static_cast<const FColor*>(Tex->PlatformData->Mips[0].BulkData.LockReadOnly());
	//TArray<FColor> pixels;
	//pixels.SetNumUninitialized(Tex->Source.GetSizeX() * Tex->Source.GetSizeY());
	//FMemory::Memcpy(pixels.GetData(), FormatedImageData, Tex->Source.GetSizeX() * Tex->Source.GetSizeY() * sizeof(FColor));
	//Tex->PlatformData->Mips[0].BulkData.Unlock();

	/*TArray<uint8> pngData;
	FImageUtils::CompressImageArray(Tex->Source.GetSizeX(), Tex->Source.GetSizeY(), pixels, pngData);
	FFileHelper::SaveArrayToFile(pngData, *imgPath);*/


	Tex->CompressionSettings = compressionSettings;
	Tex->MipGenSettings      = mipsSettings;
	Tex->SRGB                = srgb;
	Tex->UpdateResource();
	//UReimportTextureFactory::Reimport(Tex);

	//UImageWriteBlueprintLibrary::ExportToDisk

	/*TArray64<uint8> rawData;
	TArray<uint8> pngData;
	FIntPoint size = FIntPoint(Tex->Source.GetSizeX(), Tex->Source.GetSizeY());
	bool bReadSuccess = Tex->Source.GetMipData(rawData, 0);
	//CopyTextureToArray((UTexture2D*)Tex, rawData);
	if (bReadSuccess)
	{
		TArray<FColor> colors;
		colors.SetNum(size.X * size.Y);

		FMemory::Memcpy(colors.GetData(), rawData.GetData(), size.X * size.Y * sizeof(FColor));
		FImageUtils::CompressImageArray(size.X, size.Y, (TArray<FColor>)rawData, pngData);
		FFileHelper::SaveArrayToFile(pngData, *imgPath);
	}*/
}

#undef LOCTEXT_NAMESPACE
	
IMPLEMENT_MODULE(FBlockiiModule, Blockii)