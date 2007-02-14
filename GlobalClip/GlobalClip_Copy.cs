using System;
using System.Xml;
using com.amazon.s3;

namespace Extf.Net
{
    public partial class GlobalClip
    {
        public bool Copy(ClipItem item)
        {
            S3Object oItem = new S3Object(item.Data, item.MetaData);
            string copyKey = KeyPrefix + "-copy-" + oItem.GetHashCode().ToString();

            try
            {
                this.Clipboard().put(StorageBase, copyKey, oItem, null);
                this.ClipCopy.Push(item);
                return true;
            }
            catch (Exception e)
            {
                throw;
            }
        }

    }
}
