using System.Xml;

namespace  Xameleon {

  public partial class Transform {

    bool _IS_INITIALIZED = false;

    ///<summary>
    ///</summary>
    public Transform () {}

    public Context Create() {
      Context context = new Context();
      return context;
    }

    internal XmlDocument Go(Context context) {
      if (!_IS_INITIALIZED) return Process(Init(context));
      else return Process(context);
    }

  }
}
