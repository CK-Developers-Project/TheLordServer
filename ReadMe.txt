아래 항목을 모두 체크하셔야 합니다.

* 준비물
* Photon4 Server Framework 다운로드
https://www.photonengine.com/sdks#server-sdkserverserver
* Visual Stuido Framework 4.5


C# 프로젝트를 Libarary Class(Frarmwork)로 새로 생성합니다. (이름 : TheLordServer)

프로젝트 속성으로 들어가 Framework 4.5인지 확인하고
빌드탭으로 들어가 출력 경로를 설정합니다.
%파일경로%\Photon-OnPremise-Server-SDK_v4-0-29-11263\deploy\TheLordServer\bin

코드 소스를 넣고 참고를 추가합니다. or Plugins
Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\ExitGames.Logging.Log4Net.dll
Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\log4net.dll
Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\PhotonHostRuntimeInterfaces.dll
Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\Photon.SocketServer.dll
Photon-OnPremise-Server-SDK_v4-0-29-11263\lib\ExitGamesLibs.dll
protobuf-net.dll
ProtoBuf2Data.dll

Th


경로 Photon-OnPremise-Server-SDK_v4-0-29-11263\deploy\bin_Win64\PhotonServer.config 에 추가

<!-- Instance settings -->
  <TheLordServer
        MaxMessageSize="512000"
        MaxQueuedDataPerPeer="512000"
        PerPeerMaxReliableDataInTransit="51200"
        PerPeerTransmitRateLimitKBSec="256"
        PerPeerTransmitRatePeriodMilliseconds="200"
        MinimumTimeout="5000"
        MaximumTimeout="30000"
        DisplayName="TheLord" 
        >
 
    <!-- 0.0.0.0 opens listeners on all available IPs. Machines with multiple IPs should define the correct one here. -->
    <!-- Port 5055 is Photon's default for UDP connections. -->
    <UDPListeners>
      <UDPListener
                IPAddress="0.0.0.0"
                Port="5055"
                OverrideApplication="TheLord">
      </UDPListener>
    </UDPListeners>
 
    <!-- 0.0.0.0 opens listeners on all available IPs. Machines with multiple IPs should define the correct one here. -->
    <!-- Port 4530 is Photon's default for TCP connecttions. -->
    <!-- A Policy application is defined in case that policy requests are sent to this listener (known bug of some some flash clients) -->
    <TCPListeners>
      <TCPListener
                IPAddress="0.0.0.0"
                Port="4530"
                PolicyFile="Policy\assets\socket-policy.xml"
                InactivityTimeout="10000"
                OverrideApplication="TheLord"
                >
      </TCPListener>
    </TCPListeners>
 
   
 
    <!-- Defines the Photon Runtime Assembly to use. -->
    <Runtime
            Assembly="PhotonHostRuntime, Culture=neutral"
            Type="PhotonHostRuntime.PhotonDomainManager"
            UnhandledExceptionPolicy="Ignore">
    </Runtime>
 
 
    <!-- Defines which applications are loaded on start and which of them is used by default. Make sure the default application is defined. -->
    <!-- Application-folders must be located in the same folder as the bin_win32 folders. The BaseDirectory must include a "bin" folder. -->
    <Applications Default="TheLord">
 
      <!-- MMO Demo Application -->
      <Application
                Name="TheLord"
                BaseDirectory="TheLordServer"
                Assembly="TheLordServer"
                Type="TheLordServer.TheLordServer"
                ForceAutoRestart="true"
                WatchFiles="dll;config"
                ExcludeFiles="log4net.config">
      </Application>
 
    </Applications>
  </TheLordServer>