// Fill out your copyright notice in the Description page of Project Settings.

#pragma once

#include "CoreMinimal.h"
#include "UObject/NoExportTypes.h"
#include "Engine/EngineTypes.h"
#include "ImageWriteTypes.h"
#include "BlockiiSettings.generated.h"

#define PARENT_COLLECTION_NAME FName(TEXT("Blockii"))
#define MATS_COLLECTION_NAME FName(TEXT("Blockii-Mats"))
#define MESHES_COLLECTION_NAME FName(TEXT("Blockii-Meshes"))

/**
 * 
 */
UCLASS(config = Engine, defaultconfig)
class BLOCKII_API UBlockiiSettings : public UObject
{
	GENERATED_BODY()
	UBlockiiSettings();

public:
	UPROPERTY(config, EditAnywhere, Category = Settings)
	FDirectoryPath ProjectBaseDir;

	UPROPERTY(config, EditAnywhere, Category = Settings)
		TArray<FName> MaterialCollectionsToExport;

	// The tag names for material instances pramaters to use for exporting
	UPROPERTY(config, EditAnywhere, Category = Settings)
	TArray<FString> DiffuseMaterialPramaNames;

	UPROPERTY(config, EditAnywhere, Category = Settings)
	EDesiredImageFormat TextureExportFormat;

	UPROPERTY(config, EditAnywhere, Category = Settings)
	int32 TextureExportQuality;

	FString GetTextureExportBasePath();
	void CreateBaseDirs();

private:
	void CreateDirIfDoesntExist(FString Path);
};
