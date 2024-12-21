// Copyright (c) Meta Platforms, Inc. and affiliates.

#ifndef DEBUG_ENABLE_INCLUDED
#define DEBUG_ENABLE_INCLUDED

#pragma enable_d3d11_debug_symbols

void DebugEnable_float(out float Out) 
{
    Out = 1; 
}

void DebugEnable_half(out float Out)
{ 
    Out = 1; 
}

#endif
