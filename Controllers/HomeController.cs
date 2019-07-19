using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Net;
using System.Web.Mvc;
//Modelsda yarattığımız Post classını kullanmaya yarıyor.
using LogCreator.Models;
using Nest;
namespace LogCreator.Controllers
{
    public class HomeController : Controller
    {
        public ActionResult Index()
        {
            CreateNewIndex();
            SaveOnSql();
            CallRestApi();
            return View();
        }

        public void SaveOnSql()
        {
            try
            {
                // Sql veritabanına bağlantı kurmak için oluşturduğumuz nesne.
                SqlConnection sql = new SqlConnection("data source=PELINYIGIT; initial catalog=LOGO;persist security info=True;user id=sa;password=sapass; integrated security=False;MultipleActiveResultSets=True;");
                //Sql verilerini elde etmek için kullandığımız variable
                SqlCommand cmd = sql.CreateCommand();
                //verilerin get/set işlemleri
                cmd.CommandText = @"INSERT INTO DIVAAPILOG (REQUESTMETHOD, REQUESTCONTENT, REQUESTTIMESTAMP, RESPONSECONTENT, RESPONSETIMESTAMP)
                                VALUES(@RequestMethod, @RequestContent, @RequestTimestamp, @ResponseContent, @ResponseTimestamp)";

                //Belirli paramterlere var olan verileri ekleme
                cmd.Parameters.Add("@RequestMethod", SqlDbType.NVarChar, 4000).Value = "http://localhost:8282/RestService/api/products";
                cmd.Parameters.Add("@RequestContent", SqlDbType.NVarChar, 4000).Value = "<root type=\"object\"><ContractList type=\"array\"><item type=\"object\"><Code type=\"string\">stoktest{5</Code><Name type=\"string\">stoktest{5 isim</Name><TrackingCode type=\"string\">604562c1-1a68-4589-870c-c73acd2f9ffc</TrackingCode><ShortName type=\"null\"></ShortName><ProductType type=\"number\">0</ProductType><ExternalSystem type=\"number\">0</ExternalSystem><PriceList type=\"array\"><item type=\"object\"><StartDate type=\"string\">/Date(1539866698627)/</StartDate><EndDate type=\"string\">/Date(1540730698627)/</EndDate><Price type=\"number\">10</Price><Category type=\"string\">VKP0</Category><UnitCode type=\"string\">AD</UnitCode><Type type=\"number\">2</Type><Currency type=\"number\">20</Currency></item></PriceList><UnitList type=\"array\"><item type=\"object\"><Code type=\"string\">AD</Code><Name type=\"string\">Adet</Name><IsDefault type=\"boolean\">true</IsDefault><Denominator type=\"number\">1</Denominator><Numerator type=\"number\">1</Numerator></item></UnitList><BarcodeList type=\"array\"><item type=\"object\"><Code type=\"string\">1234567</Code><Index type=\"number\">1</Index><IsMain type=\"boolean\">true</IsMain><UnitCode type=\"string\">AD</UnitCode></item></BarcodeList><TaxList type=\"null\"></TaxList><TenantId type=\"string\">59513b70-6182-48ec-8222-1e14ebf866cf</TenantId><ContextId type=\"null\"></ContextId><ContextModuleId type=\"number\">1</ContextModuleId><Id type=\"null\"></Id><Description type=\"null\"></Description><IsActive type=\"boolean\">false</IsActive><InsertedDate type=\"null\"></InsertedDate><InsertedBy type=\"null\"></InsertedBy><UpdatedDate type=\"null\"></UpdatedDate><UpdatedBy type=\"null\"></UpdatedBy><CustomFields type=\"null\"></CustomFields><TagInformations type=\"array\"><item type=\"object\"><Name type=\"string\">E5</Name><CategoryInformation type=\"object\"><Name type=\"string\">K1</Name></CategoryInformation></item><item type=\"object\"><Name type=\"string\">E5</Name><CategoryInformation type=\"object\"><Name type=\"string\">K1</Name></CategoryInformation></item></TagInformations></item></ContractList></root>";
                cmd.Parameters.Add("@RequestTimestamp", DateTime.Now);
                cmd.Parameters.Add("@ResponseContent", SqlDbType.NVarChar, 4000).Value = "<root type=\"object\"><FailList type=\"array\"></FailList><Message type=\"null\"></Message><Success type=\"boolean\">true</Success><SuccessList type=\"array\"><item type=\"object\"><Key>stoktest{5</Key><Value>OK</Value></item></SuccessList></root>";
                cmd.Parameters.Add("@ResponseTimestamp", DateTime.Now.AddSeconds(2));

                //Bağlantıyı açma, işleme ve kapatma
                sql.Open();
                cmd.ExecuteNonQuery();
                sql.Close();
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
                throw e;
            }

        }
        
        
        public static void CreateNewIndex()
        {
            //Elasticsearch Uri'yı ile bağlantı kurma ve bu uri üzerinde oluşturduğumuz Post.cs verilerinin mapping bağlantısını sağlama ve yeni index oluşturma.
            ConnectionSettings settings =
            new ConnectionSettings(new Uri("http://localhost:9200/"))
                  .DefaultMappingFor<Post>(m => m
                  .IndexName("my_blog"));


            //NEST objesi yaratıldı.
            ElasticClient client = new ElasticClient(settings);

            //Modelda oluşturduğumuz Post.cs classının variableları için veriler eklendi.
            var post = new Post()
            {
                PostDate = DateTime.Now,
                PostTest = "Begining Angular2",
                UserID = 1
            };

            var post2 = new Post()
            {
                
                PostDate = DateTime.Now,
                PostTest = "What is Elastic Search",
                UserID = 2
            };

            var post3 = new Post()
            {
                PostDate = DateTime.Now,
                PostTest = "kibana",
                UserID = 3
            };

            //Eklediğimiz Post verilerinin, yarattığımız "my_blog" indexine atılması. 
            client.Index<Post>(post, idx => idx.Index("my_blog"));
            client.Index<Post>(post2, idx => idx.Index("my_blog"));
            client.Index<Post>(post3, idx => idx.Index("my_blog"));

            //Search yaratmak için yaratılan "response" querysinin filtreleme mimarisinin gösterilmesi. 
            var response = client.Search<Post>(p => p
              .From(0)
              .Size(5)
              .Query(q =>
              q.Term(f => f.UserID, 2)
              || q.MatchPhrasePrefix(mq => mq.Field(f => f.PostTest).Query("angular")))
         
              );
        }
           //RestFUL api'yı .NET'e çağırmak için ürettiğimiz controller methodu.
            public static void CallRestApi()
        {
            //Bağlamak istediğimiz verilerin alındığı uri
            string s = string.Format("http://localhost:9200/my_blog/_search?pretty");
            //Web bağlantısı için gereken fonksiyonların kullanımı.
            HttpWebRequest request = (HttpWebRequest)WebRequest.Create(s);
            //Search için kullanılan elasticsearch terimi olan GET methodunun kullanımı. 
            request.Method = "GET";
            //Oluşturduğumuz querylerin alınması.
            HttpWebResponse response = (HttpWebResponse)request.GetResponse();

            //Aldığımız querylerin alınıp TextReader sayesinde işlenmesi ve searchlenmesi.
            using (Stream stream = response.GetResponseStream())
            {
                StreamReader sr = new StreamReader(stream);
                var sresult = sr.ReadToEnd();
                sr.Close();
            }
        }
    }

}
