// MysticLight_Test.cpp : Defines the entry point for the console application.
// Copyright © 2020 Micro-Star INT'L CO., LTD.

#include "stdafx.h"

typedef int (*LPMLAPI_Initialize)();
typedef int (*LPMLAPI_Release)();
typedef int (*LPMLAPI_GetDeviceInfo)(SAFEARRAY** pDevType, SAFEARRAY** pLedCount);
typedef int (*LPMLAPI_GetDeviceName)(BSTR type, SAFEARRAY** pDevName);
typedef int (*LPMLAPI_GetDeviceNameEx)(BSTR type, DWORD index, BSTR* pDevName);
typedef int (*LPMLAPI_GetErrorMessage)(int ErrorCode, BSTR* pDesc);
typedef int (*LPMLAPI_GetLedName)(BSTR type, SAFEARRAY** pLedName);
typedef int (*LPMLAPI_GetLedInfo)(BSTR type, DWORD index, BSTR* pName, SAFEARRAY** pLedStyles);
typedef int (*LPMLAPI_GetLedColor)(BSTR type, DWORD index, DWORD* R, DWORD* G, DWORD* B);
typedef int (*LPMLAPI_GetLedStyle)(BSTR type, DWORD index, BSTR* style);
typedef int (*LPMLAPI_GetLedMaxBright)(BSTR type, DWORD index, DWORD* maxLevel);
typedef int (*LPMLAPI_GetLedBright)(BSTR type, DWORD index, DWORD* currentLevel);
typedef int (*LPMLAPI_GetLedMaxSpeed)(BSTR type, DWORD index, DWORD* maxLevel);
typedef int (*LPMLAPI_GetLedSpeed)(BSTR type, DWORD index, DWORD* currentLevel);
typedef int (*LPMLAPI_SetLedColor)(BSTR type, DWORD index, DWORD R, DWORD G, DWORD B);
typedef int (*LPMLAPI_SetLedColors)(BSTR type, DWORD AreaIndex, SAFEARRAY** pLedName, DWORD* R, DWORD* G, DWORD* B);
typedef int (*LPMLAPI_SetLedColorEx)(BSTR type, DWORD AreaIndex, BSTR pLedName, DWORD R, DWORD G, DWORD B, DWORD );
typedef int (*LPMLAPI_SetLedColorSync)(BSTR type, DWORD AreaIndex, BSTR pLedName, DWORD R, DWORD G, DWORD B, DWORD );
typedef int (*LPMLAPI_SetLedStyle)(BSTR type, DWORD index, BSTR style);
typedef int (*LPMLAPI_SetLedBright)(BSTR type, DWORD index, DWORD level);
typedef int (*LPMLAPI_SetLedSpeed)(BSTR type, DWORD index, DWORD level);
typedef void(*CallbackDelegate)();
typedef int (*LPMLAPI_MysticLightControlNotify)(CallbackDelegate *callbackFunc);
typedef int (*LPMLAPI_SetLedColorsSync)(BSTR type, DWORD R, DWORD G, DWORD B);

CallbackDelegate cb_MysticLightControlNotify()
{
	printf_s("Mystic Light is Controlling.");

	return 0;
}

int _tmain(int argc, _TCHAR* argv[])
{	
	HMODULE hLibrary = LoadLibrary(_T("MysticLight_SDK.dll"));

	if(hLibrary == NULL){
		printf_s("ERROR: Load Library Failed.\n");
		return S_FALSE;
	}

	LPMLAPI_Initialize lpMLAPI_Initialize;
	LPMLAPI_GetErrorMessage lpMLAPI_GetErrorMessage;
	LPMLAPI_GetDeviceInfo lpMLAPI_GetDeviceInfo;
	LPMLAPI_GetLedInfo lpMLAPI_GetLedInfo;
	LPMLAPI_GetLedColor lpMLAPI_GetLedColor;
	LPMLAPI_SetLedColor lpMLAPI_SetLedColor;
	LPMLAPI_GetLedStyle lpMLAPI_GetLedStyle;	
	LPMLAPI_SetLedStyle lpMLAPI_SetLedStyle;
	LPMLAPI_MysticLightControlNotify lpMLAPI_MysticLightControlNotify;
	LPMLAPI_SetLedColorsSync lpMLAPI_SetLedColorsSync;
	
	lpMLAPI_Initialize=(LPMLAPI_Initialize)GetProcAddress(hLibrary,"MLAPI_Initialize");
	lpMLAPI_GetErrorMessage=(LPMLAPI_GetErrorMessage)GetProcAddress(hLibrary,"MLAPI_GetErrorMessage");
	lpMLAPI_GetDeviceInfo=(LPMLAPI_GetDeviceInfo)GetProcAddress(hLibrary,"MLAPI_GetDeviceInfo");
	lpMLAPI_GetLedInfo=(LPMLAPI_GetLedInfo)GetProcAddress(hLibrary,"MLAPI_GetLedInfo");
	lpMLAPI_GetLedColor=(LPMLAPI_GetLedColor)GetProcAddress(hLibrary,"MLAPI_GetLedColor");
	lpMLAPI_SetLedColor=(LPMLAPI_SetLedColor)GetProcAddress(hLibrary,"MLAPI_SetLedColor");
	lpMLAPI_GetLedStyle=(LPMLAPI_GetLedStyle)GetProcAddress(hLibrary,"MLAPI_GetLedStyle");	
	lpMLAPI_SetLedStyle=(LPMLAPI_SetLedStyle)GetProcAddress(hLibrary,"MLAPI_SetLedStyle");
	lpMLAPI_MysticLightControlNotify=(LPMLAPI_MysticLightControlNotify)GetProcAddress(hLibrary,"MLAPI_MysticLightControlNotify");
	lpMLAPI_SetLedColorsSync=(LPMLAPI_SetLedColorsSync)GetProcAddress(hLibrary,"MLAPI_SetLedColorsSync");

	if( lpMLAPI_Initialize == NULL		||
		lpMLAPI_GetErrorMessage == NULL	||
		lpMLAPI_GetDeviceInfo == NULL	||
		lpMLAPI_GetLedInfo == NULL		||
		lpMLAPI_GetLedColor == NULL		||
		lpMLAPI_SetLedColor == NULL		||
		lpMLAPI_GetLedStyle == NULL		||		
		lpMLAPI_SetLedStyle == NULL		||
		lpMLAPI_MysticLightControlNotify == NULL ||
		lpMLAPI_SetLedColorsSync == NULL)
	{
		printf_s("Unable to find DLL function.\n");
	}
	else
	{
		if(lpMLAPI_Initialize() != S_OK)
		{
			printf("MysticLight_SDK.dll initialization failed.\n");
			return 0;
		}

		int MLAPI_Status;
		BSTR MLAPI_StatusDesc;
		printf_s("MysticLight_SDK.dll Initialized.\n\n");

		// Register Notification
		lpMLAPI_MysticLightControlNotify((CallbackDelegate*)cb_MysticLightControlNotify);

		// Enumerate the devices type and LED counts
		SAFEARRAY *pDevType, *pLedCount;
		MLAPI_Status = lpMLAPI_GetDeviceInfo(&pDevType, &pLedCount);
		if(MLAPI_Status == S_OK)
		{
			printf_s("Enumerate the devices type and LED counts. (Defined type, LED count):\n\n");
			for(int i=0; i<pDevType->rgsabound->cElements; i++)
			{
				printf_s("(%S, %S)\n", ((BSTR*)(pDevType->pvData))[i], ((BSTR*)(pLedCount->pvData))[i]);
			}
		}

		// query LED display name
		printf_s("\nPress enter to get %S[%d] LED display name ...\n", ((BSTR*)(pDevType->pvData))[0], 0);
		getchar();
		BSTR LED_Name;
		SAFEARRAY* LED_Styles;
		MLAPI_Status = lpMLAPI_GetLedInfo(((BSTR*)(pDevType->pvData))[0], 0, &LED_Name, &LED_Styles);
		printf_s("%S[%d]:\"%S\"\n", ((BSTR*)(pDevType->pvData))[0], 0, LED_Name);

		// query LED current color
		printf_s("\nPress enter to get %S[%d] LED color ...\n", ((BSTR*)(pDevType->pvData))[0], 0);
		getchar();
		DWORD R, G, B;
		MLAPI_Status = lpMLAPI_GetLedColor(((BSTR*)(pDevType->pvData))[0], 0, &R, &G, &B);
		printf_s("%S[%d]: RGB(%d,%d,%d)\n", ((BSTR*)(pDevType->pvData))[0], 0, R, G, B);

		// query LED current style
		printf_s("\nPress enter to get %S[%d] LED style ...\n", ((BSTR*)(pDevType->pvData))[0], 0);
		getchar();
		BSTR LED_Style;
		MLAPI_Status = lpMLAPI_GetLedStyle(((BSTR*)(pDevType->pvData))[0], 0, &LED_Style);
		printf_s("%S[%d]:\"%S\"\n", ((BSTR*)(pDevType->pvData))[0], 0, LED_Style);

		// query LED support styles
		printf_s("\nPress enter to get %S[%d] LED support style ...\n", ((BSTR*)(pDevType->pvData))[0], 0);
		getchar();
		printf_s("%S[%d]:\n", ((BSTR*)(pDevType->pvData))[0], 0);
		for(ULONG i = 0; i < LED_Styles->rgsabound->cElements; i++)
		{
			printf_s("\"%S\"\n", ((BSTR*)(LED_Styles->pvData))[i]);
		}
		
		// set LED color and style
		printf_s("\nPress enter to set %S[%d] LED to yellow color and breathing style...\n", ((BSTR*)(pDevType->pvData))[0], 0);
		getchar();
		MLAPI_Status = lpMLAPI_SetLedColor(((BSTR*)(pDevType->pvData))[0], 0, 255, 255, 0);
		lpMLAPI_GetErrorMessage(MLAPI_Status, &MLAPI_StatusDesc);
		printf_s("Set color: %S\n", MLAPI_StatusDesc);	
		MLAPI_Status = lpMLAPI_SetLedStyle(((BSTR*)(pDevType->pvData))[0], 0, L"Breathing");
		lpMLAPI_GetErrorMessage(MLAPI_Status, &MLAPI_StatusDesc);
		printf_s("Set style: %S\n", MLAPI_StatusDesc);

		// set LED color and style to default setting
		printf_s("\nPress enter to set %S[%d] LED to to white color and no animation style...\n", ((BSTR*)(pDevType->pvData))[0], 0);
		getchar();
		MLAPI_Status = lpMLAPI_SetLedColor(((BSTR*)(pDevType->pvData))[0], 0, 255, 255, 255);
		lpMLAPI_GetErrorMessage(MLAPI_Status, &MLAPI_StatusDesc);
		printf_s("Set color: %S\n", MLAPI_StatusDesc);
		MLAPI_Status = lpMLAPI_SetLedStyle(((BSTR*)(pDevType->pvData))[0], 0, L"No animation");
		lpMLAPI_GetErrorMessage(MLAPI_Status, &MLAPI_StatusDesc);
		printf_s("Set style: %S\n", MLAPI_StatusDesc);

		printf_s("\nPress any key to exit ...\n");
		getchar();
	}

	FreeLibrary(hLibrary);

	return 0;
}

