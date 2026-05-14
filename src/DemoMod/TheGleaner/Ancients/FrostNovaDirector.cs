using System;
using BaseLib.Abstracts;
using BaseLib.Utils;
using DemoMod.TheGleaner.Relics;
using Godot;
using MegaCrit.Sts2.Core.Models;

namespace DemoMod.TheGleaner.Ancients;

public class FrostNovaDirector : CustomAncientModel
{
	public static FrostNovaDirector? Instance { get; private set; }

	public FrostNovaDirector() : base(autoAdd: true, logDialogueLoad: true)
	{
		Instance = this;
	}

	public override bool IsValidForAct(ActModel act)
	{
		string entry = act?.Id?.Entry ?? "";
		return entry.Equals("HIVE", StringComparison.OrdinalIgnoreCase);
	}

	public override string CustomScenePath =>
		"res://TheGleaner/scenes/ancients/frost_nova_director.tscn";

	public override string CustomMapIconPath =>
		"res://TheGleaner/images/ancients/frost_nova_director/map_icon.png";

	public override string CustomMapIconOutlinePath =>
		"res://TheGleaner/images/ancients/frost_nova_director/map_icon_outline.png";


	protected override OptionPools MakeOptionPools
	{
		get
		{
			return new OptionPools(
				CustomAncientModel.MakePool(
					CustomAncientModel.AncientOption<Telescope>()
					
				),
				CustomAncientModel.MakePool(
					CustomAncientModel.AncientOption<Handkerchief>(),
					CustomAncientModel.AncientOption<BrittleStar>()
				),
				CustomAncientModel.MakePool(
					CustomAncientModel.AncientOption<IcedBlueberry>(),
					CustomAncientModel.AncientOption<GoldenSilkPouch>()

				)
			);
		}
	}
}
