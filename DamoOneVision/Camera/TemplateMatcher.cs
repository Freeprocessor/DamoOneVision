using Matrox.MatroxImagingLibrary;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace DamoOneVision.Camera
{
	public class TemplateMatcher : IDisposable
	{
		private MIL_ID MilSystem = MIL.M_NULL;

		private MIL_ID MilTemplateImage = MIL.M_NULL;
		private MIL_ID MilPatContext = MIL.M_NULL;
		private MIL_ID MilPatResult = MIL.M_NULL;

		public TemplateMatcher( )
		{
			// MIL 애플리케이션 및 시스템 초기화
			//MIL.MappAlloc( MIL.M_DEFAULT, ref MilApplication );
			//MIL.MsysAlloc( MIL.M_SYSTEM_DEFAULT, MIL.M_NULL, MIL.M_DEFAULT, ref MilSystem );

			// 패턴 매칭 컨텍스트 및 결과 할당
			//MIL.MpatAlloc( MilSystem, MIL.M_NORMALIZED, MIL.M_DEFAULT, ref MilPatContext );
			//MIL.MpatAllocResult( MilSystem, MIL.M_DEFAULT, ref MilPatResult );
			MilSystem = MILContext.Instance.MilSystem;

			// 패턴 매칭 컨텍스트 및 결과 할당
			MIL.MpatAlloc( MilSystem, MIL.M_NORMALIZED, MIL.M_DEFAULT, ref MilPatContext );
			MIL.MpatAllocResult( MilSystem, MIL.M_DEFAULT, ref MilPatResult );
		}

		public void TeachTemplate( byte[ ] imageData, int width, int height )
		{
			// 템플릿 이미지 버퍼 생성
			MIL.MbufAlloc2d( MilSystem, width, height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref MilTemplateImage );

			// 이미지 데이터를 MIL 버퍼에 복사
			MIL.MbufPut( MilTemplateImage, imageData );

			// 템플릿 정의
			MIL.MpatDefine( MilPatContext, MIL.M_REGULAR_MODEL, MilTemplateImage, 0, 0, width, height, MIL.M_DEFAULT );
		}

		public void FindTemplate( byte[ ] imageData, int width, int height, out double posX, out double posY, out double score )
		{
			posX = 0;
			posY = 0;
			score = 0;

			// 입력 이미지 버퍼 생성
			MIL_ID MilInputImage = MIL.M_NULL;
			MIL.MbufAlloc2d( MilSystem, width, height, 8 + MIL.M_UNSIGNED, MIL.M_IMAGE + MIL.M_PROC, ref MilInputImage );

			// 이미지 데이터를 MIL 버퍼에 복사
			MIL.MbufPut( MilInputImage, imageData );

			// 패턴 매칭 수행
			MIL.MpatFind( MilPatContext, MilInputImage, MilPatResult );

			// 결과 가져오기
			double numOccurrences = 0;
			MIL.MpatGetResult( MilPatResult, MIL.M_NUMBER + MIL.M_TYPE_DOUBLE, ref numOccurrences );

			if (numOccurrences > 0)
			{
				MIL.MpatGetResult( MilPatResult, MIL.M_POSITION_X + MIL.M_TYPE_DOUBLE, 0,ref posX );
				MIL.MpatGetResult( MilPatResult, MIL.M_POSITION_Y + MIL.M_TYPE_DOUBLE, 0, ref posY);
				MIL.MpatGetResult( MilPatResult, MIL.M_SCORE + MIL.M_TYPE_DOUBLE, 0, ref score );
			}

			// 입력 이미지 버퍼 해제
			MIL.MbufFree( MilInputImage );
		}

		public void Dispose( )
		{
			// 리소스 해제
			if (MilPatResult != MIL.M_NULL)
				MIL.MpatFree( MilPatResult );

			if (MilPatContext != MIL.M_NULL)
				MIL.MpatFree( MilPatContext );

			if (MilTemplateImage != MIL.M_NULL)
				MIL.MbufFree( MilTemplateImage );

			//if (MilSystem != MIL.M_NULL)
			//	MIL.MsysFree( MilSystem );

			//if (MilApplication != MIL.M_NULL)
			//	MIL.MappFree( MilApplication );
		}
	}
}
