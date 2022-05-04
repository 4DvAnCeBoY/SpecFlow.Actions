﻿using BoDi;
using OpenQA.Selenium;
using Specflow.Actions.Browserstack;
using SpecFlow.Actions.Browserstack;
using SpecFlow.Actions.Browserstack.DriverInitialisers;
using SpecFlow.Actions.Selenium;
using SpecFlow.Actions.Selenium.Configuration;
using SpecFlow.Actions.Selenium.DriverInitialisers;
using SpecFlow.Actions.Selenium.Hoster;
using System;
using System.Linq;
using TechTalk.SpecFlow;
using TechTalk.SpecFlow.Plugins;
using TechTalk.SpecFlow.UnitTestProvider;

[assembly: RuntimePlugin(typeof(BrowserstackRuntimePlugin))]

namespace Specflow.Actions.Browserstack
{
    public class BrowserstackRuntimePlugin : IRuntimePlugin
    {
        public void Initialize(RuntimePluginEvents runtimePluginEvents, RuntimePluginParameters runtimePluginParameters,
            UnitTestProviderConfiguration unitTestProviderConfiguration)
        {
            runtimePluginEvents.CustomizeScenarioDependencies += RuntimePluginEvents_CustomizeScenarioDependencies;
            runtimePluginEvents.CustomizeGlobalDependencies += RuntimePluginEvents_CustomizeGlobalDependencies;
        }

        private void RuntimePluginEvents_CustomizeGlobalDependencies(object? sender, CustomizeGlobalDependenciesEventArgs e)
        { 
            var runtimePluginTestExecutionLifecycleEventEmitter = e.ObjectContainer.Resolve<RuntimePluginTestExecutionLifecycleEvents>();
            runtimePluginTestExecutionLifecycleEventEmitter.AfterScenario += RuntimePluginTestExecutionLifecycleEventEmitter_AfterScenario;
            runtimePluginTestExecutionLifecycleEventEmitter.AfterTestRun += RuntimePluginTestExecutionLifecycleEventEmitter_AfterTestRun;
        }

        private void RuntimePluginTestExecutionLifecycleEventEmitter_AfterTestRun(object sender, RuntimePluginAfterTestRunEventArgs e)
        {
            BrowserstackLocalService.Stop();
        }

        private void RuntimePluginTestExecutionLifecycleEventEmitter_AfterScenario(object? sender, RuntimePluginAfterScenarioEventArgs e)
        {
            var scenarioContext = e.ObjectContainer.Resolve<ScenarioContext>();
            var browserDriver = e.ObjectContainer.Resolve<BrowserDriver>();

            if (scenarioContext.ScenarioExecutionStatus == ScenarioExecutionStatus.OK)
            {

                ((IJavaScriptExecutor)browserDriver.Current).ExecuteScript(BrowserstackTestResultExecutor.GetResultExecutor("passed"));
            }
            else
            {
                ((IJavaScriptExecutor)browserDriver.Current).ExecuteScript(BrowserstackTestResultExecutor.GetResultExecutor("failed", scenarioContext.TestError.Message));
            }
        }

        private void RuntimePluginEvents_CustomizeScenarioDependencies(object? sender, CustomizeScenarioDependenciesEventArgs e)
        {
            e.ObjectContainer.RegisterTypeAs<BrowserstackConfiguration, ISeleniumConfiguration>();
            e.ObjectContainer.RegisterTypeAs<BrowserstackCredentialProvider, ICredentialProvider>();
            RegisterInitialisers(e.ObjectContainer);
        }

        private void RegisterInitialisers(IObjectContainer objectContainer)
        {
            objectContainer.RegisterFactoryAs<IDriverInitialiser>(container =>
            {
                var config = container.Resolve<ISeleniumConfiguration>();

                if (((BrowserstackConfiguration)config).BrowserstackLocalRequired)
                {
                    BrowserstackLocalService.Start(
                        ((BrowserstackConfiguration)config).BrowserstackLocalCapabilities.ToList());
                }

                var browserstackDriverInitialiser = container.Resolve<BrowserstackDriverInitialiser>();
                var credentialProvider = container.Resolve<ICredentialProvider>();
                return config.Browser switch
                {
                    Browser.Chrome => new BrowserstackChromeDriverInitialiser(browserstackDriverInitialiser, config, credentialProvider),
                    Browser.Firefox => new BrowserstackFirefoxDriverInitialiser(browserstackDriverInitialiser, config, credentialProvider),
                    Browser.Edge => new BrowserstackEdgeDriverInitialiser(browserstackDriverInitialiser, config, credentialProvider),
                    Browser.InternetExplorer => new BrowserstackInternetExplorerDriverInitialiser(browserstackDriverInitialiser, config, credentialProvider),
                    Browser.Safari => new BrowserstackSafariDriverInitialiser(browserstackDriverInitialiser, config, credentialProvider),
                    _ => throw new ArgumentOutOfRangeException($"Browser {config.Browser} not implemented")
                };
            });
        }
    }
}