using DamoOneVision.Camera;
using DamoOneVision.Data;
using DamoOneVision.Services;
using DamoOneVision.Services.Input;
using DamoOneVision.Services.Repository;
using DamoOneVision.ViewModels;
using DamoOneVision.Views;
using KeypadDemo.Services;
using Matrox.MatroxImagingLibrary;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using System.Configuration;
using System.Data;
using System.Diagnostics;
using System.Windows;

namespace DamoOneVision
{
	/// <summary>
	/// Interaction logic for App.xaml
	/// </summary>
	public partial class App : Application
	{
		// DI 컨테이너
		public IServiceProvider ServiceProvider { get; private set; }
		private static MILContext _milContextRef = MILContext.Instance;

		protected override void OnStartup( StartupEventArgs e )
		{
			base.OnStartup( e );

			// DI 컨테이너 설정
			var services = new ServiceCollection();
			ConfigureServices( services );
			ServiceProvider = services.BuildServiceProvider();

			// MainWindow를 DI 컨테이너에서 resolve하고 표시
			var mainWindow = ServiceProvider.GetRequiredService<MainWindow>();
			mainWindow.Show();
		}

		private void ConfigureServices( IServiceCollection services )
		{
			services.AddSingleton<IModelRepository, JsonModelRepository>();

			// 카메라 인스턴스(여러 카메라가 있으므로, 미리 생성하여 등록)
			var infraredCamera = new CameraManager("Matrox", "InfraredCamera");
			var sideCamera1 = new CameraManager("Matrox", "SideCamera1");
			var sideCamera2 = new CameraManager("Matrox", "SideCamera2");
			var sideCamera3 = new CameraManager("Matrox", "SideCamera3");
			services.AddSingleton( infraredCamera );
			services.AddSingleton( sideCamera1 );
			services.AddSingleton( sideCamera2 );
			services.AddSingleton( sideCamera3 );

			// 기타 서비스 등록
			var modbus = new ModbusService("192.168.2.100", 502);
			services.AddSingleton( modbus );

			var motionService = new MotionService();
			services.AddSingleton( motionService );

			var advantechCard = new AdvantechCardService("192.168.2.20", 502);
			services.AddSingleton( advantechCard );

			// MilSystemService는 MIL 관련 초기화를 담당
			var milSystemService = new MilSystemService();
			services.AddSingleton( milSystemService );

			// CameraService: MIL display ID는 MilSystemService에서 가져옴
			services.AddSingleton<CameraService>( sp => new CameraService(
				infraredCamera,
				sideCamera1,
				sideCamera2,
				sideCamera3,
				milSystemService,
				new Lazy<MainViewModel>( ( ) => sp.GetRequiredService<MainViewModel>())
				
			) );

			// DeviceControlService
			services.AddSingleton<DeviceControlService>( sp => new DeviceControlService(
				modbus, advantechCard, motionService
			) );

			services.AddSingleton<SettingManager>( sp => new SettingManager(
				sp.GetRequiredService<DeviceControlService>(),
				sp.GetRequiredService<CameraService>()
			) );

			// ViewModel들
			services.AddSingleton<MainViewModel>( sp => new MainViewModel(
				sp.GetRequiredService<DeviceControlService>(),
				sp.GetRequiredService<CameraService>(),
				sp.GetRequiredService<SettingManager>()
			) );
			services.AddSingleton<ManualViewModel>( sp => new ManualViewModel(
				sp.GetRequiredService<DeviceControlService>(),
				motionService,
				sp.GetRequiredService<CameraService>(),
				sp.GetRequiredService<SettingManager>()
			) );

			services.AddSingleton<SettingViewModel>( sp => new SettingViewModel(
				sp.GetRequiredService<SettingManager>(),
				sp.GetRequiredService<CameraService>(),
				sp.GetRequiredService<MainViewModel>()
			) );


			// MainWindow: 생성자에 MainViewModel, ManualViewModel, MilSystemService 주입
			services.AddSingleton<MainWindow>( sp => new MainWindow(
				sp.GetRequiredService<MainViewModel>(),
				sp.GetRequiredService<ManualViewModel>(),
				sp.GetRequiredService<SettingViewModel>(),
				milSystemService,
				sp.GetRequiredService<CameraService>()
			) );
		}

		protected override async void OnExit( ExitEventArgs e )
		{
			base.OnExit( e );

			try
			{
				Debug.WriteLine( "[App] OnExit 시작" );

				ServiceProvider.GetRequiredService<CameraService>()?.Dispose();
				ServiceProvider.GetRequiredService<MotionService>()?.ReleaseLibrary();

				var milSystemService = ServiceProvider.GetRequiredService<MilSystemService>();
				await milSystemService.DisposeAsync();

				MILContext.Instance.Dispose();

				Logger.Shutdown();
				Debug.WriteLine( "[App] OnExit 종료" );
			}
			catch (Exception ex)
			{
				Debug.WriteLine( "[OnExit 예외] " + ex.Message );
				Debug.WriteLine( ex.StackTrace );
			}
		}
	}
}