using System;
using com.amazon.s3;

namespace Extf.Net
{
    public partial class GlobalClip
    {
        public bool Init()
        {
            
            /// This is a hack. FIXME FIRST.
            /// 
            if (!debug)
            {
                this.GC_PUBLIC_KEY = appSet_Private_Public_Keys[1];
                this.GC_PRIVATE_KEY = appSet_Private_Public_Keys[0];
            }
            else
            {
                this.GC_PUBLIC_KEY = debugPublicKey;
                this.GC_PRIVATE_KEY = debugPrivateKey;
            }

            try
            {
                this.Connect = new AWSAuthConnection(GC_PUBLIC_KEY, GC_PRIVATE_KEY);
                this.Connected = true;
                return true;
            }
            catch (Exception e)
            {
                throw;
            }

        }

    }
}
