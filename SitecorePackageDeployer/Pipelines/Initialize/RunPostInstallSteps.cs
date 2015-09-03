﻿using Hhogdev.SitecorePackageDeployer.Metadata;
using Hhogdev.SitecorePackageDeployer.Tasks;
using Sitecore.Configuration;
using Sitecore.Diagnostics;
using Sitecore.Pipelines;
using System;
using System.IO;
using System.Xml.Serialization;

namespace Hhogdev.SitecorePackageDeployer.Pipelines.Initialize
{
    public class RunPostInstallSteps
    {
        string _packageSource;

        public RunPostInstallSteps()
        {
            LoadSettings();
        }

        private void LoadSettings()
        {
            _packageSource = InstallPackage.GetPackageSource();
        }

        public void Process(PipelineArgs args)
        {
            try
            {
                RunPostInitializeStepsIfNeeded();
            }
            catch(Exception ex)
            {
                Log.Error("Failed to complete post initialize steps", ex, this);
            }
        }

        private void RunPostInitializeStepsIfNeeded()
        {
            string startupPostStepPackageFile = Path.Combine(_packageSource, InstallPackage.STARTUP_POST_STEP_PACKAGE_FILENAME);

            //remove post step flag file if it exists
            if (File.Exists(startupPostStepPackageFile))
            {
                try
                {
                    //Load the post step details
                    XmlSerializer serializer = new XmlSerializer(typeof(PostStepDetails));
                    using (TextReader writer = new StreamReader(startupPostStepPackageFile))
                    {
                        PostStepDetails details = serializer.Deserialize(writer) as PostStepDetails;

                        if (details != null)
                        {
                            InstallPackage.ExecutePostSteps(details);
                        }
                    }
                }
                finally
                {
                    //cleanup the post step
                    File.Delete(startupPostStepPackageFile);
                }
            }
        }
    }
}
