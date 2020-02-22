// Fill out your copyright notice in the Description page of Project Settings.

#include "BlockiiSettings.h"

#include "Misc/Paths.h"
#include "HAL/PlatformFilemanager.h"

UBlockiiSettings::UBlockiiSettings()
{
	DiffuseMaterialPramaNames.Add(FString(TEXT("")));
	DiffuseMaterialPramaNames.Add(FString(TEXT("Albedo")));
}

FString UBlockiiSettings::GetTextureExportBasePath()
{
	return FPaths::Combine(ProjectBaseDir.Path, FString(TEXT("Textures")));
}

void UBlockiiSettings::CreateBaseDirs()
{
	CreateDirIfDoesntExist(GetTextureExportBasePath());
}

void UBlockiiSettings::CreateDirIfDoesntExist(FString Path)
{
	/*IPlatformFile& PlatformFile = FPlatformFileManager::Get().GetPlatformFile();

	if (!PlatformFile.DirectoryExists(Path))
	{
		PlatformFile.CreateDirectory(Path);
	}*/
}
