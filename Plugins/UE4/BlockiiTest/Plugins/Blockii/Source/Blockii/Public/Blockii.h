// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Modules/ModuleManager.h"

DECLARE_LOG_CATEGORY_EXTERN(Blockii, Log, Log);

class FToolBarBuilder;
class FMenuBuilder;
class UBlockiiSettings;

class FBlockiiModule : public IModuleInterface
{
public:

	/** IModuleInterface implementation */
	virtual void StartupModule() override;
	virtual void ShutdownModule() override;
	
	/** This function will be bound to Command. */
	void PluginButtonClicked();

	UBlockiiSettings* Settings;
	void GetMatsInCollection();
	void ExportMat(FName CollectionName, FAssetData AssetData);
	void ExportTexture(UTexture* Tex, FName CollectionName, FName MatName);
	
private:

	void AddToolbarExtension(FToolBarBuilder& Builder);
	void AddMenuExtension(FMenuBuilder& Builder);
	void RegisterSettings();
	void UnRegisterSettings();

	static void CopyTextureToArray(UTexture2D* Texture, TArray<FColor>& Array);

private:
	TSharedPtr<class FUICommandList> PluginCommands;
};
