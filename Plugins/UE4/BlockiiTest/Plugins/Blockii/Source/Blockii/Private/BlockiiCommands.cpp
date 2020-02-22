// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#include "BlockiiCommands.h"

#define LOCTEXT_NAMESPACE "FBlockiiModule"

void FBlockiiCommands::RegisterCommands()
{
	UI_COMMAND(PluginAction, "Blockii", "Execute Blockii action", EUserInterfaceActionType::Button, FInputGesture());
}

#undef LOCTEXT_NAMESPACE
