using System;
using System.Dynamic;
using System.Collections.Generic;
using System.Xml;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;
using Habitat.Client.Tests.TestClasses;

namespace Habitat.Client.Tests
{
    [TestClass]
    public class ValidationTests
    {
        [TestMethod]
        public void Test_boolean_validation()
        {
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidBoolean("taco"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidBoolean("true"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidBoolean("False"));
        }

        [TestMethod]
        public void Test_email_address_validation()
        {
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidEmailAddress("taco"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidEmailAddress("test123@example.com"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidEmailAddress("Im.the.curator@example.museum"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidEmailAddress("Im.the.curator@example.museum,test123@example.com"));
        }

        [TestMethod]
        public void Test_email_address_list_validation()
        {
            Assert.IsTrue(SampleApplicationConfigProvider.AreAllValidEmailAddresses("Im.the.curator@example.museum;test123@example.com", ';'));
            Assert.IsTrue(SampleApplicationConfigProvider.AreAllValidEmailAddresses("Im.the.curator@example.museum", ';'));
            Assert.IsFalse(SampleApplicationConfigProvider.AreAllValidEmailAddresses(";Im.the.curator@example.museum", ';'));
            Assert.IsFalse(SampleApplicationConfigProvider.AreAllValidEmailAddresses("Im.the.curator@example.museum;;", ';'));
            Assert.IsFalse(SampleApplicationConfigProvider.AreAllValidEmailAddresses("Im.the.curator@example.museum ; ; Im.the.curator@example.museum ;", ';'));
            Assert.IsFalse(SampleApplicationConfigProvider.AreAllValidEmailAddresses("blah", ';'));
            Assert.IsFalse(SampleApplicationConfigProvider.AreAllValidEmailAddresses("blah;Im.the.curator@example.museum", ';'));
            Assert.IsTrue(SampleApplicationConfigProvider.AreAllValidEmailAddresses("Im.the.curator@example.museum,test123@example.com", ','));
        }

        [TestMethod]
        public void Test_exists_validation()
        {
            Assert.IsTrue(SampleApplicationConfigProvider.Exists(null));
            Assert.IsTrue(SampleApplicationConfigProvider.Exists(string.Empty));
            Assert.IsTrue(SampleApplicationConfigProvider.Exists(new object()));
            Assert.IsTrue(SampleApplicationConfigProvider.Exists("foo"));
        }

        [TestMethod]
        public void Test_hostname_or_ip_validation()
        {
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostnameOrIp(null));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostnameOrIp(string.Empty));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostnameOrIp("   "));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostnameOrIp("123"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostnameOrIp("255.255.255.256"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostnameOrIp("abc"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidHostnameOrIp(" 123.123.123.123 "));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidHostnameOrIp("0.0.0.0"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostnameOrIp(" 0.0.-1.0"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidHostname(" www.yahoo.com "));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidHostname(" r2d2.protk.com"));
        }

        [TestMethod]
        public void Test_hostname_validation()
        {
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostname(null));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostname(string.Empty));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostname("   "));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostname("123"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostname("foobar "));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostname("123.123"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostname("123.123.123"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostname("123.123.123.123"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidHostname("abc"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidHostname(" www.yahoo.com "));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidHostname(" r2d2.protk.com"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidHostname(" protk.com"));
        }

        [TestMethod]
        public void Test_ip_address_validation()
        {
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress(null));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress(string.Empty));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress("   "));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress("123"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress("123.123"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress("123.123.123"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidIpAddress("123.123.123.123"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress("256.255.255"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress("255.255.255.256"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress("abc"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidIpAddress(" 123.123.123.123 "));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidIpAddress("0.0.0.0"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidIpAddress(" 0.0.-1.0"));
        }

        [TestMethod]
        public void Test_interval_validation()
        {
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidTimeInterval("0.0.0.0"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidTimeInterval("abc"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidTimeInterval(""));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidTimeInterval(null));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidTimeInterval(" "));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidTimeInterval("1:1"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidTimeInterval("1:1:1"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidTimeInterval("01:01:01"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidTimeInterval("12:12"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidTimeInterval("25:16"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidTimeInterval("23:16:61"));
        }

        [TestMethod]
        public void Test_sql_connection_string_validation()
        {
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=myServerName\myInstanceName;Database=myDataBase;User Id=myUsername;Password=myPassword;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Data Source=myServerAddress;Initial Catalog=myDataBase;Integrated Security=SSPI;User ID=myDomain\myUsername;Password=myPassword;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Data Source=190.190.200.100,1433;Network Library=DBMSSOCN;Initial Catalog=myDataBase;User ID=myUsername;Password=myPassword;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;MultipleActiveResultSets=true;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=.\SQLExpress;AttachDbFilename=C:\MyFolder\MyDataFile.mdf;Database=dbname;Trusted_Connection=Yes;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=.\SQLExpress;AttachDbFilename=|DataDirectory|mydbfile.mdf;Database=dbname;Trusted_Connection=Yes;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=(localdb)\v11.0;Integrated Security=true;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=(localdb)\v11.0;Integrated Security=true;AttachDbFileName=C:\MyFolder\MyData.mdf;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=(localdb)\MyInstance;Integrated Security=true;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=np:\\.\pipe\LOCALDB#F365A78E\tsql\query;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=(localdb)\.\MyInstanceShare;Integrated Security=true;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Data Source=myServerAddress;Failover Partner=myMirrorServerAddress;Initial Catalog=myDataBase;Integrated Security=True;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=myServerAddress;Database=myDataBase;Integrated Security=True;Asynchronous Processing=True;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Data Source=.\SQLExpress;Integrated Security=true;AttachDbFilename=C:\MyFolder\MyDataFile.mdf;User Instance=true;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=myServerAddress;Database=myDataBase;User ID=myUsername;Password=myPassword;Trusted_Connection=False;Packet Size=4096;"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"   Server=myServerAddress;Database=myDataBase;Trusted_Connection=True;   "));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"foo"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(null));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@" "));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(string.Empty));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=myServerName/myInstanceName;Database=myDataBase;UserId=myUsername;Password=myPassword;"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=myServerName\myInstanceName;Database=myDataBase;UserId=myUsername;"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSqlServerConnectionString(@"Server=myServerName\myInstanceName;UserId=myUsername;Password=myPassword;"));
        }

        [TestMethod]
        public void Test_integer_validation()
        {
            dynamic original = new ExpandoObject();
            original.Number = 5;
            var serializedOriginal = JsonConvert.SerializeObject(original);
            dynamic newObject = JsonConvert.DeserializeObject<ExpandoObject>(serializedOriginal);
            var dictionary = newObject as IDictionary<string, object> ?? new Dictionary<string, object>();
            var testValue = dictionary["Number"];
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidInteger(testValue));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidInteger("taco"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidInteger("145"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidInteger(long.MaxValue.ToString()));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidInteger(int.MaxValue.ToString()));
        }

        [TestMethod]
        public void Test_url_validation()
        {
            dynamic original = new ExpandoObject();
            original.Url = new Uri("http://www.google.com");
            var serializedOriginal = JsonConvert.SerializeObject(original);
            dynamic newObject = JsonConvert.DeserializeObject<ExpandoObject>(serializedOriginal);
            var dictionary = newObject as IDictionary<string, object> ?? new Dictionary<string, object>();
            var testValue = dictionary["Url"];
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidUrl(testValue));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidUrl("taco"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidUrl("http://www.google.com"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidUrl(long.MaxValue.ToString()));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidUrl("ftp://192.168.254.1"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidUrl("ftp:/192.168.254.1"));
        }

        [TestMethod]
        public void Test_path_validation()
        {
            dynamic original = new ExpandoObject();
            original.Path = @"C:\Protk\";
            var serializedOriginal = JsonConvert.SerializeObject(original);
            dynamic newObject = JsonConvert.DeserializeObject<ExpandoObject>(serializedOriginal);
            var dictionary = newObject as IDictionary<string, object> ?? new Dictionary<string, object>();
            var testValue = dictionary["Path"];
            Assert.IsTrue(SampleApplicationConfigProvider.IsWellFormedPath(testValue));
            Assert.IsFalse(SampleApplicationConfigProvider.IsWellFormedPath("taco^#>"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsWellFormedPath(@"c:\"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsWellFormedPath(long.MaxValue.ToString()));
            Assert.IsTrue(SampleApplicationConfigProvider.IsWellFormedPath(AppDomain.CurrentDomain.BaseDirectory));
            Assert.IsFalse(SampleApplicationConfigProvider.IsWellFormedPath("ftp://192.168.254.1"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsWellFormedPath(@"\\protk.com\shares"));
        }

        [TestMethod]
        public void Test_xml_validation()
        {
            Assert.IsFalse(SampleApplicationConfigProvider.IsWellFormedXml("taco^#>"));
            var testDoc = new XmlDocument();
            var elem = testDoc.CreateElement("Testing");
            var attrib = testDoc.CreateAttribute("attrib");
            attrib.Value = "1";
            elem.SetAttributeNode(attrib);
            testDoc.AppendChild(elem);
            Assert.IsTrue(SampleApplicationConfigProvider.IsWellFormedXml(testDoc.OuterXml));
            Assert.IsFalse(SampleApplicationConfigProvider.IsWellFormedXml(long.MaxValue.ToString()));
            Assert.IsFalse(SampleApplicationConfigProvider.IsWellFormedXml(AppDomain.CurrentDomain.BaseDirectory));
        }

        [TestMethod]
        public void Test_input_queue_validation()
        {
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidInputQueueName("ProTeck.Junction_Input@localhost"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidInputQueueName("ProTeck.Junction_input@localhost"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidInputQueueName("ProTeck.Junction_input@192.168.1.1"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidInputQueueName("ProTeck.Junction_input@qa-app1"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidInputQueueName("ProTeck.junction_InPuT"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidInputQueueName("ProTeck.Junction_pubqueue@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidInputQueueName("ProTeck.Junction.input@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidInputQueueName("ProTeck_Input"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidInputQueueName("Junction_Input@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidInputQueueName(string.Empty));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidInputQueueName(null));
        }

        [TestMethod]
        public void Test_error_queue_validation()
        {
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidErrorQueueName("ProTeck.Junction_Error@localhost"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidErrorQueueName("ProTeck.Junction_error@localhost"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidErrorQueueName("ProTeck.Junction_error@192.168.1.1"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidErrorQueueName("ProTeck.Junction_error@qa-app1"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidErrorQueueName("ProTeck.junction_ErRoR"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidErrorQueueName("ProTeck.Junction_errorqueue@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidErrorQueueName("ProTeck.Junction.error@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidErrorQueueName("ProTeck_Error"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidErrorQueueName("Junction_Error@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidErrorQueueName(string.Empty));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidErrorQueueName(null));
        }

        [TestMethod]
        public void Test_subscription_queue_validation()
        {
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("ProTeck.Junction_Subscriptions@localhost"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("ProTeck.Junction_subscriptions@localhost"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("ProTeck.Junction_subscriptions@192.168.1.1"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("ProTeck.Junction_subscriptions@qa-app1"));
            Assert.IsTrue(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("ProTeck.junction_SuBsCrIpTiOns"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("ProTeck.Junction_pubqueue@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("ProTeck.Junction.subscriptions@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("ProTeck.Junction_subscription@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("ProTeck_Subscriptions"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSubscriptionQueueName("Junction_Subscriptions@localhost"));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSubscriptionQueueName(string.Empty));
            Assert.IsFalse(SampleApplicationConfigProvider.IsValidSubscriptionQueueName(null));
        }
    }
}
