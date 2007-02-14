using System;
using System.Configuration;

namespace Extf.Net
{
    public partial class GlobalClip
    {
        private string _GC_PUBLIC_KEY;
        private string GC_PUBLIC_KEY
        {
            get
            {
                return this._GC_PUBLIC_KEY;
            }
            set
            {
                this._GC_PUBLIC_KEY = value;
            }
        }
        private string _GC_PRIVATE_KEY;
        private string GC_PRIVATE_KEY
        {
            get
            {
                return this._GC_PRIVATE_KEY;
            }
            set
            {
                this._GC_PRIVATE_KEY = value;
            }
        }
    }
}
