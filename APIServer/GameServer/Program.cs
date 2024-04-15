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

        //Program은 거시적인 설정(HTTP 서버 등 한번 설정하고 나면 거의 바뀌지 않음)
        //Startup은 세부적인 설정(미들웨어, Dependency Injection 등 필요에 의해 추가/변화)
        

        public static void Main(string[] args)
        {
            //3.IHost를 만든다
            //4.구동(Run) < 이때부터 Listen을 시작 
            CreateHostBuilder(args).Build().Run();
        }

        //1. 각종 옵션 설정을 세팅
        public static IHostBuilder CreateHostBuilder(string[] args) =>
            Host.CreateDefaultBuilder(args)
                .ConfigureWebHostDefaults(webBuilder =>
                {
                    //2. startup 클래스 지정
                    webBuilder.UseStartup<Startup>();
                });
    }
}
