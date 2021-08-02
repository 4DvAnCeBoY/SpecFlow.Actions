﻿// unset

using SpecFlow.Actions.Configuration;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly:RuntimePlugin(typeof(ConfigurationRuntimePlugin))]

namespace SpecFlow.Actions.Configuration
{
    public class ConfigurationRuntimePlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeScenarioDependencies += RuntimePluginEvents_CustomizeScenarioDependencies;
        }

        private void RuntimePluginEvents_CustomizeScenarioDependencies(object sender, CustomizeScenarioDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<SpecFlowActionJsonLocator, ISpecFlowActionJsonLocator>();
            e.ObjectContainer.RegisterTypeAs<SpecFlowActionJsonLoader, ISpecFlowActionJsonLoader>();
        }
    }
}