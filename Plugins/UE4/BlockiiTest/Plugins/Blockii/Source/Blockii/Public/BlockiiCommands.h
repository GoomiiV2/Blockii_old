// Copyright 1998-2019 Epic Games, Inc. All Rights Reserved.

#pragma once

#include "CoreMinimal.h"
#include "Framework/Commands/Commands.h"
#include "BlockiiStyle.h"

class FBlockiiCommands : public TCommands<FBlockiiCommands>
{
public:

	FBlockiiCommands()
		: TCommands<FBlockiiCommands>(TEXT("Blockii"), NSLOCTEXT("Contexts", "Blockii", "Blockii Plugin"), NAME_None, FBlockiiStyle::GetStyleSetName())
	{
	}

	// TCommands<> interface
	virtual void RegisterCommands() override;

public:
	TSharedPtr< FUICommandInfo > PluginAction;
};
