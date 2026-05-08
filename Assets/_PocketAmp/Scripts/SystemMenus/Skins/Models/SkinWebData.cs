using System.Collections.Generic;
using System;

namespace SoftAware.PocketAmp.SystemMenus.Skins
{
    [Serializable]
    public class SkinData
    {
        public string id;
        public string title;
        public string thumbnail_url;
        public string download_url;
    }

    [Serializable]
    public class SkinListResponse
    {
        public List<SkinData> items;
        public int total;
        public int page;
    }
}
