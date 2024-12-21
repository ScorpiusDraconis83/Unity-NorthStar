// Copyright (c) Meta Platforms, Inc. and affiliates.

package com.meta.northstar;

public interface ProfileCommandInterface
{
    // void setScene(String scene);
    // void setCamera(String camera);
    // void setProfilingEnabled(boolean enabled);

    void setString(String key, String value);
    void setFloat(String key, float value);
    void setInteger(String key, int value);
    void setBoolean(String key, boolean value);
}