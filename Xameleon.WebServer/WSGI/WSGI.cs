// Copyright (c) 2006 by Seo Sanghyeon
// Copyright (c) 2006 by Christopher Baus http://baus.net/

// 2006-03-30 sanxiyn Created
// 2006-07-13 Mark Rees Modified for hosting api changes
// 2006-10-03 Mark Rees Use shared IronPython engine
// 2006-10-31 Christopher Baus 
//            Refactored to use one assembly to implement
//            both shared and reloaded IronPython interpreters.
//
//            Also made handler configurable through the AppSettings in
//            web.config.
using System;
using System.IO;
using System.Web;
using IronPython.Hosting;
using IronPython.Runtime;
using System.Configuration;
using System.Threading;

public class WSGIHandler : IHttpHandler
{
    public const string DEFAULT_MODULE = "wsgi_app";
    public const string DEFAULT_APP = "app";
    public const string DEFAULT_APP_URL_PATH = "";


    private static PythonEngine engine;
    private static bool modulesLoaded = false;
    private static String moduleName;

    static WSGIHandler()
    {
        String reloadVal = ConfigurationManager.AppSettings["WSGIReloadIronPython"];
        if (reloadVal == null || reloadVal.ToLower() != "true")
        {
            engine = CreateIronPythonEngine();
        }
        else
        {
            engine = null;
        }
        moduleName = ConfigurationManager.AppSettings["WSGIApplicationModule"];
        if (moduleName == null || moduleName == "")
        {
            moduleName = DEFAULT_MODULE;
        }

    }

    public static PythonEngine CreateIronPythonEngine()
    {
        EngineOptions engineOptions = new EngineOptions();
        engineOptions.ShowClrExceptions = true;
        return new PythonEngine(engineOptions);
    }


    public void SetPath(PythonEngine engine, String rootPath)
    {
        engine.AddToPath(rootPath);

        // This path entry allows the framework modules to be loaded from 
        // rootPath/bin/Lib
        engine.Sys.prefix = Path.Combine(rootPath, "bin");
        string libpath =  Path.Combine(Path.Combine(rootPath, "bin"), "Lib");
        engine.AddToPath(libpath);
        
        // This path entry allows the application specific modules to be 
        // loaded from rootPath/wsgiapp.  This allows seperation of the
        // the framework and application.
        engine.AddToPath(Path.Combine(rootPath, "wsgiapp"));
    }

    public void LoadModules(PythonEngine engine, String appModule)
    {
        engine.Import("site");
        engine.Import("wsgi");
        engine.Import(appModule);
    }

    public void ProcessRequest(HttpContext context)
    {
        PythonEngine requestEngine = engine;
        if (requestEngine == null)
        {
            requestEngine = CreateIronPythonEngine();
        }
        Monitor.Enter(this.GetType());
        try
        {

            if (!modulesLoaded || engine == null)
            {
                SetPath(requestEngine, context.Request.PhysicalApplicationPath);
                LoadModules(requestEngine, moduleName);
                modulesLoaded = true;
            }

        }
        finally
        {
            Monitor.Exit(this.GetType());
        }

        string application_name = ConfigurationManager.AppSettings["WSGIApplication"];
        if (application_name == null || application_name == "")
        {
            application_name = DEFAULT_APP;
        }

        string application_url_path = ConfigurationManager.AppSettings["WSGIApplicationURLPath"];
        if (application_url_path == null || application_url_path == "")
        {
            application_url_path = DEFAULT_APP_URL_PATH;
        }


        while (application_url_path.EndsWith("/"))
        {
            // The trailing /'s should be stripped from the application path, so "/" is passed
            // as the root of the PATH_INFO environ variable.	
            application_url_path = application_url_path.TrimEnd('/');
        }

        System.Collections.Generic.IDictionary<string, object> locals = new
            System.Collections.Generic.Dictionary<string, object>();
        locals["context"] = context;
        String code = String.Format("wsgi.run_application(context, {0}.{1}, '{2}')",
                                                   moduleName,
                                                   application_name,
                                                   application_url_path);
        requestEngine.Execute(code,
                              requestEngine.DefaultModule,
                              locals);
    }

    public bool IsReusable
    {
        get
        {
            return true;
        }
    }
}

