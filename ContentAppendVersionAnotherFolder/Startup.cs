using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.HttpsPolicy;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.FileProviders;
using Microsoft.Extensions.Hosting;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Threading.Tasks;

namespace ContentAppendVersionAnotherFolder
{
	public class Startup
	{
		public Startup(IConfiguration configuration)
		{
			Configuration = configuration;
		}

		public IConfiguration Configuration { get; }

		// This method gets called by the runtime. Use this method to add services to the container.
		public void ConfigureServices(IServiceCollection services)
		{
			services.AddRazorPages();
		}

		// This method gets called by the runtime. Use this method to configure the HTTP request pipeline.
		public void Configure(IApplicationBuilder app, IWebHostEnvironment env)
		{
			if (env.IsDevelopment())
			{
				app.UseDeveloperExceptionPage();
			}
			else
			{
				app.UseExceptionHandler("/Error");
				// The default HSTS value is 30 days. You may want to change this for production scenarios, see https://aka.ms/aspnetcore-hsts.
				app.UseHsts();
			}

			app.UseHttpsRedirection();

			// ��������C��

			var staticOptions = new StaticFileOptions[]
			{
				// Site1 �p�̕����R���e���c�t�H���_�ƎQ�� URL ��R�Â���
				new StaticFileOptions()
				{
					FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Areas/Site1/Content")),
					RequestPath = "/Site1",
				},
				// ��������ꍇ�͂���Ȋ����Œǉ�
				//new StaticFileOptions()
				//{
				//	FileProvider = new PhysicalFileProvider(Path.Combine(env.ContentRootPath, "Areas/Site2/Content")),
				//	RequestPath = "/Site2",
				//},
			};

			// wwwroot �t�H���_�ŐÓI�t�@�C���Q�Ƃ�L���ɂ���
			app.UseStaticFiles();

			// �ǉ������� StaticFileOptions
			foreach (var option in staticOptions)
			{
				app.UseStaticFiles(option);
			}

			// StaticFileOptions ��Ǝ��N���X�ł܂Ƃ߂� WebRootFileProvider �ɃZ�b�g����
			var compositeProvider = new CompositeStaticFileOptionsProvider(env.WebRootFileProvider, staticOptions);
			env.WebRootFileProvider = compositeProvider;

			// �����܂ŏC��

			app.UseRouting();

			app.UseAuthorization();

			app.UseEndpoints(endpoints =>
			{
				endpoints.MapRazorPages();
			});
		}
	}
}
