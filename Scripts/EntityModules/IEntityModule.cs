﻿using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace EemRdx.EntityModules
{
    public interface IEntityModule
    {
        string DebugModuleName { get; }
        string DebugFullName { get; }
    }
}
