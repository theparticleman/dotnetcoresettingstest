using NUnit.Framework;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Configuration.Json;
using System.Collections.Generic;
using System.IO;
using Microsoft.AspNetCore.Hosting.Internal;
using Microsoft.AspNetCore.Hosting;
using System;

namespace Tests
{
    public class ConfigurationTests
    {
        ConfigurationBuilder builder;

        [SetUp]
        public void SetUp()
        {
            builder = new ConfigurationBuilder();
        }

        [Test]
        public void InMemory()
        {
            builder.AddInMemoryCollection(new Dictionary<string, string> {
                {"setting1", "setting 1 value"},
                {"setting2", "setting 2 value"}
            });

            var config = builder.Build();

            Assert.That(config["setting1"], Is.EqualTo("setting 1 value"));
            Assert.That(config["setting2"], Is.EqualTo("setting 2 value"));
        }

        [Test]
        public void JsonFile()
        {
            builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json");

            var config = builder.Build();

            Assert.That(config["setting1"], Is.EqualTo("setting 1 value"));
            Assert.That(config["setting2"], Is.EqualTo("setting 2 value"));
        }

        [Test]
        public void DevelopmentEnvironment()
        {
            builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Development.json");

            var config = builder.Build();

            Assert.That(config["setting1"], Is.EqualTo("setting 1 value"));
            Assert.That(config["setting2"], Is.EqualTo("setting 2 value for Development"));
        }

        [Test]
        public void MultipleEnvironments()
        {
            builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile("appsettings.Staging.json")
            .AddJsonFile("appsettings.Production.json");

            var config = builder.Build();

            Assert.That(config["setting1"], Is.EqualTo("setting 1 value for Staging"));
            Assert.That(config["setting2"], Is.EqualTo("setting 2 value for Production"));

        }

        [Test]
        public void SettingsFileThatDoesNotExist()
        {
            builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("does-not-exist.json");

            Assert.Throws<FileNotFoundException>(() => builder.Build());
        }

        [Test]
        public void BindToPoco()
        {
            builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("bindToPoco.json");

            var config = builder.Build();
            var postgresConfig = new PostgresConfig();
            config.GetSection("Postgres").Bind(postgresConfig);

            Assert.That(postgresConfig.Username, Is.EqualTo("foo"));
            Assert.That(postgresConfig.Password, Is.EqualTo("super secret password"));
        }

        [Test]
        public void FromEnvironmentVariables()
        {
            builder.AddEnvironmentVariables();

            var config = builder.Build();

            Assert.That(config["os"], Is.EqualTo("Windows_NT"));
            Assert.That(config["path"], Is.Not.Empty);
        }

        [Test]
        public void EnvironmentDefaultsToProduction()
        {
            var env = new HostingEnvironment();

            Assert.That(env.EnvironmentName, Is.EqualTo("Production"));
            Assert.That(env.IsDevelopment(), Is.False);
        }

        [Test]
        public void SettingsBasedOnCurrentEnvironment()
        {
            var env = new HostingEnvironment();

            builder
            .SetBasePath(Directory.GetCurrentDirectory())
            .AddJsonFile("appsettings.json")
            .AddJsonFile($"appsettings.{env.EnvironmentName}.json");

            var config = builder.Build();

            Assert.That(config["setting1"], Is.EqualTo("setting 1 value"));
            Assert.That(config["setting2"], Is.EqualTo("setting 2 value for Production"));
        }
    }

    public class PostgresConfig
    {
        public string Username { get; set; }
        public string Password { get; set; }
    }
}