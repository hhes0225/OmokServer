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

        //���� ���� �߰�(DI)
        // This method gets called by the runtime. Use this method to add services to the container.
        public void ConfigureServices(IServiceCollection services)
        {
            services.AddControllersWithViews();
        }
        //DI ���񽺶�? SRP(Single Responsibility Principle)
        //ex) ��ŷ ���� ����� �ʿ��ϸ�? -> ��ŷ ���� ����

        //HTTP request pipeline(Node js�� ����)
        //�̵����: HTTP request/response�� ó���ϴ� �߰� ��ǰ

        //[Request]                            [Response]
        //[����������]                         [����������]
        //              [������ MVC EndPoint]

        //�̵����� ó���� ������� �ٸ� �̵����� �ѱ� �� �ִ�.
        //[����������]

        //[!] Controller���� ó������ ���� ������?
        //ex) ��� ��û���� �α��� �ؾ� �Ѵٸ�?

        //� HTTP ��û�� ���� ��, ���� ��� �����ϴ��� �Ϸ��� �������� ��Ÿ��
        //1) IIS, Apache � HTTP ��û
        //2) ASP.Net Core ����(Kestrel) ����
        //3) �̵���� ����
        //4) Controller�� ����
        //5) Controller���� ó���ϰ� View�� ����
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

            //CSS, Javascript, �̹��� �� ��û ���� �� ó��
            app.UseStaticFiles();

            app.UseRouting();

            app.UseAuthorization();


            //����� ���� ����
            app.UseEndpoints(endpoints =>
            {
                endpoints.MapControllerRoute(
                    name: "default",
                    pattern: "{controller=Home}/{action=Index}/{id?}");
            });
        }
    }
}
