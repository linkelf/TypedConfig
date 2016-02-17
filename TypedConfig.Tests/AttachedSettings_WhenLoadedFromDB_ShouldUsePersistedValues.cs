using System.Net.Mail;
using System.Reflection;
using Domain;
using NUnit.Framework;
using PersistedAttachedProperties;
using PersistedAttachedPropertiesConcrete.Persistance;
using Ploeh.AutoFixture;

namespace TypedConfig.Tests
{
    [TestFixture]
    public class AttachedSettings_WhenLoadedFromDB_ShouldUsePersistedValues : AttachedSettings_Persisting_Tests
    {
        private ConfigGeneratorDummy _existingConfig;

        private class ConfigGeneratorDummy : IExternalConfig_For_VK
        {
            public string FirstName { get; set; }
            public string LastName { get; set; }
            public string MiddleName { get; set; }
            public decimal MounthlyFee { get; set; }
            public decimal Balance { get; set; }
            public MailAddress CustomerMail { get; set; }
            public SubscriptionType Subscription { get; set; }
        }

        protected override void Given()
        {
            base.Given();

            var fixture = (new Fixture());
            _existingConfig = fixture.Create<ConfigGeneratorDummy>();

            PersistExistingConfig();
        }

        private void PersistExistingConfig()
        {
            using (var context = new PropertyContext())
            {
                foreach (
                    var prop in typeof (IExternalConfig_For_VK).GetProperties(BindingFlags.Instance | BindingFlags.Public))
                {
                    var p = context.DomainEntityAttachedProperties.Add(new AttachedProperty
                    {
                        EntityType = typeof (IExternalConfig_For_VK).FullName,
                        Name = prop.Name,
                        Type = prop.PropertyType.FullName
                    });

                    context.SaveChanges();

                    context.DomainEntityAttachedPropertyValuesInt.Add(new AttachedPropertyValue<int>
                    {
                        EntityId = EntityId,
                        PropertyId = p.Id,
                        Value = prop.GetValue(_existingConfig).ToString()
                    });

                    context.SaveChanges();
                }
            }
        }

        [Test]
        public void Then_FirstName_is_taken_from_persisted_storage()
        {
            Assert.AreEqual(_existingConfig.FirstName, Config.FirstName);
        }

        [Test]
        public void Then_Mail_is_taken_from_persisted_storage()
        {
            Assert.AreEqual(_existingConfig.CustomerMail, Config.CustomerMail);
        }
    }
}