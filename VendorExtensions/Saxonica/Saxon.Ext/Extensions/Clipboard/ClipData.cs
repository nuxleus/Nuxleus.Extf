using System;
using System.Xml;
using Saxon.Api;
using System.IO;

namespace Saxon.Ext {

  public partial class Function {

    public string base64Data { get { return this._base64Data; } set { this._base64Data = value; } }
    private string _base64Data;

    public void SetClip (string base64) {
      this.base64Data = base64;
    }
    public string GetClip () {
      return this.base64Data;
    }
  }
}


