using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace WebApplication2
{
    public class Startup
    {
        public Startup(IConfiguration configuration)
        {
            Configuration = configuration;
        }

        public IConfiguration Configuration { get; }

        //각종 서비스 추가(DI)
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }
        //DI 서비스란? SRP(Single Responsibility Principle)
        //ex) 랭킹 관련 기능이 필요하면? -> 랭킹 서비스 생성

        //HTTP request pipeline(Node js와 유사)
        //미들웨어: HTTP request/response를 처리하는 중간 부품

        //[Request]                            [Response]
        //[파이프라인]                         [파이프라인]
        //              [마지막 MVC EndPoint]

        //미들웨어에서 처리한 결과물을 다른 미들웨어로 넘길 수 있다.
        //[파이프라인]

        //[!] Controller에서 처리하지 않은 이유는?
        //ex) 모든 요청마다 로깅을 해야 한다면?

        //어떤 HTTP 요청이 왔을 때, 앱이 어떻게 응답하는지 일련의 과정들을 나타냄
        //1) IIS, Apache 등에 HTTP 요청
        //2) ASP.Net Core 서버(Kestrel) 전달
        //3) 미들웨어 적용
        //4) Controller로 전달
        //5) Controller에서 처리하고 View로 전달
        // This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
        public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
        {
            if (env.IsDevelopment())
            {
                app.UseDeveloperExceptionPage();
            }
            else
            {
                app.UseExceptionHandler("/Home/Error");
                // The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
                app.UseHsts();
            }
            app.UseHttpsRedirection();

            //CSS, Javascript, 이미지 등 요청 받을 때 처리
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();


            //라우팅 패턴 설정
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
