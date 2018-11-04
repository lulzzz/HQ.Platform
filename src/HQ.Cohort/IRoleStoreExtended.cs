﻿// Copyright (c) HQ Corporation. All rights reserved.
// Licensed under the Reciprocal Public License, Version 1.5. See LICENSE.md in the project root for license terms.

using System.Threading;
using Microsoft.AspNetCore.Identity;

namespace HQ.Cohort
{
	public interface IRoleStoreExtended<TRole> : IRoleStore<TRole> where TRole : class
	{
		CancellationToken CancellationToken { get; }
	}
}