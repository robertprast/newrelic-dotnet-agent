/*
* Copyright 2020 New Relic Corporation. All rights reserved.
* SPDX-License-Identifier: Apache-2.0
*/
#pragma once
#include "IModule.h"

namespace NewRelic {
    namespace Profiler {
        namespace ModuleInjector
        {
            class ModuleInjector
            {
            public:
                static void InjectIntoModule(IModule& module)
                {
                    return;
                }
            };
        }
    }
}
