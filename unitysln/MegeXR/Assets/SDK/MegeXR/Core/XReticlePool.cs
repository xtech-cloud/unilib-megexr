/********************************************************************
     Copyright (c) XTech Cloud
     All rights reserved.
*********************************************************************/

using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

namespace XTC.MegeXR.Core
{
    public class XReticlePool
    {
		public static Dictionary<string, XReticleAgent> reticles = new Dictionary<string, XReticleAgent>();

		public static void Clean()
		{
			reticles.Clear();
		}

		public static void Register(XReticleAgent _reticle)
		{
			if(reticles.ContainsKey(_reticle.uuid))
				return;
			reticles.Add(_reticle.uuid, _reticle);
		}

		public static void Cancel(string _uuid)
		{
			if(!reticles.ContainsKey(_uuid))
				return;
			reticles.Remove(_uuid);
		}

		public static XReticleAgent Find(string _uuid)
		{
			if(!reticles.ContainsKey(_uuid))
				return null;
			return reticles[_uuid];
		}

	}
}