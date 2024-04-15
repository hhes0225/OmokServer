using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2
{
    public class Program
    {

        //Program�� �Ž����� ����(HTTP ���� �� �ѹ� �����ϰ� ���� ���� �ٲ��� ����)
        //Startup�� �������� ����(�̵����, Dependency Injection �� �ʿ信 ���� �߰�/��ȭ)
        

        public static void Main(string[] args)
        {
            //3.IHost�� �����
            //4.����(Run) < �̶����� Listen�� ���� 
            CreateHostBuilder(args).Build().Run();
        }

        //1. ���� �ɼ� ������ ����
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //2. startup Ŭ���� ����
                    webBuilder.UseStartup<Startup>();
                });
    }
}
