// CopyRight (c) CMyna. All Rights Preserved.
// file "IMoonTextureRenderer.cs".
// Licensed under MIT License.

using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;

namespace BetterMoonLight.API
{
    public interface IMoonTextureRenderer
    {

        public bool Render(RenderTexture target);
    }

}
