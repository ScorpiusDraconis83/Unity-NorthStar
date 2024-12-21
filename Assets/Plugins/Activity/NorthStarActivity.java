// Copyright (c) Meta Platforms, Inc. and affiliates.

package com.meta.northstar;
import com.unity3d.player.UnityPlayerActivity;
import android.os.Bundle;
import android.util.Log;
import android.content.IntentFilter;

public class NorthStarActivity extends UnityPlayerActivity 
{
	ProfileCommandReceiver profileCommandReceiver;

	protected void onCreate(Bundle savedInstanceState) 
	{
		super.onCreate(savedInstanceState);

		profileCommandReceiver = new ProfileCommandReceiver();
		registerReceiver(profileCommandReceiver, new IntentFilter("com.meta.northstar"));
	}	
}