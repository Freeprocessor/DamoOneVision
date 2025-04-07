using DamoOneVision.Camera;
using DamoOneVision.Data;
using DamoOneVision.Services;
using DamoOneVision.ViewModels;
using DamoOneVision.Views;
using Matrox.MatroxImagingLibrary;
using Microsoft.Extensions.DependencyInjection;
using System.Configuration;
using System.Data;
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
				 new Lazy<MainViewModel>( ( ) => sp.GetRequiredService<MainViewModel>() )
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
				sp.GetRequiredService<CameraService>()
			) );

			services.AddSingleton<SettingViewModel>( sp => new SettingViewModel(
				sp.GetRequiredService<SettingManager>(),
				sp.GetRequiredService<CameraService>()
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

			// DI 컨테이너에서 resolve한 서비스들로 Dispose 호출

			// 카메라 서비스 Dispose
			var cameraService = ServiceProvider.GetRequiredService<CameraService>();
			cameraService?.Dispose();

			// MotionService: 해제 메서드 호출
			var motionService = ServiceProvider.GetRequiredService<MotionService>();
			motionService.ReleaseLibrary();

			// 각 카메라 DisconnectAsync (여기서는 카메라 인스턴스를 직접 resolve)
			var infraredCamera = ServiceProvider.GetRequiredService<CameraManager>();
			var sideCamera1 = ServiceProvider.GetRequiredService<CameraManager>(); // 첫 번째 CameraManager가 InfraredCamera이므로
																				   // 만약 여러 카메라가 필요하다면, 별도로 관리할 수 있도록 타입을 분리하는 것이 좋습니다.
																				   // 여기서는 간단히 예시로만 처리합니다.
			await infraredCamera.DisconnectAsync();
			await sideCamera1.DisconnectAsync();
			// sideCamera2, sideCamera3 등도 필요하면 추가 호출

			// MilSystemService 비동기 Dispose (IAsyncDisposable 구현)
			var milSystemService = ServiceProvider.GetRequiredService<MilSystemService>();
			if (milSystemService is IAsyncDisposable asyncDisposable)
			{
				await asyncDisposable.DisposeAsync();
			}
			MILContext.Instance.Dispose();

			await Task.Delay( 100 );
			Logger.Shutdown();

			// DI 컨테이너에 등록된 객체들이 모두 Dispose되도록 할 수 있다면,
			// (서비스 컨테이너가 IDisposable를 구현하는 경우) ServiceProvider.Dispose();

			Application.Current.Shutdown();
		}
	}
}