using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace KFLauncher.Models
{
    internal static class DefaultConfigs
    {
        public const string KillingFloorIni = @"[URL]
Protocol=KillingFloor
ProtocolDescription=KillingFloor Protocol
Name=ROSoldier
Map=Index.rom
LocalMap=KFIntro.rom
NetBrowseMap=Entry.rom
Host=
Portal=
MapExt=rom
EXEName=KillingFloor.exe
SaveExt=usa
Port=7707
Class=KFMod.KFHumanPawn
Character=Corporal_Lewis

[UPNP]
Enabled=True

[FirstRun]
FirstRun=3339

[Engine.Engine]
RenderDevice=D3D9Drv.D3D9RenderDevice
;RenderDevice=D3DDrv.D3DRenderDevice
;RenderDevice=Engine.NullRenderDevice
;RenderDevice=OpenGLDrv.OpenGLRenderDevice
;RenderDevice=PixoDrv.PixoRenderDevice
AudioDevice=ALAudio.ALAudioSubsystem
NetworkDevice=IpDrv.TcpNetDriver
DemoRecordingDevice=Engine.DemoRecDriver
Console=KFMod.KFConsole
GUIController=KFGUI.KFGUIController
StreamPlayer=Engine.StreamInteraction
Language=int
Product=KillingFloor
GameEngine=Engine.GameEngine
EditorEngine=Editor.EditorEngine
DefaultGame=KFMod.KFGameType
DefaultServerGame=KFMod.KFGameType
ViewportManager=WinDrv.WindowsClient
;ViewportManager=SDLDrv.SDLClient
Render=Render.Render
Input=Engine.Input
Canvas=Engine.Canvas
DetectedVideoMemory=-2147483648
ServerReadsStdin=False
NVidiaGPU=False
NVidiaPreFX=False

[Core.System]
PurgeCacheDays=30
SavePath=..\Save
CachePath=../Cache
CacheExt=.uxx
CacheRecordPath=../System/*.ucl
MusicPath=../Music
SpeechPath=../Speech
Paths=../System/*.u
Paths=../Maps/*.rom
Paths=../TestMaps/*.rom
Paths=../Textures/*.utx
Paths=../Sounds/*.uax
Paths=../Music/*.umx
Paths=../StaticMeshes/*.usx
Paths=../Animations/*.ukx
Paths=../Saves/*.uvx
Suppress=DevLoad
Suppress=DevSave
Suppress=DevNetTraffic
Suppress=DevGarbage
Suppress=DevKill
Suppress=DevReplace
Suppress=DevCompile
Suppress=DevBind
Suppress=DevBsp
Suppress=DevNet
Suppress=DevLIPSinc
Suppress=DevKarma
Suppress=RecordCache
Suppress=MapVoteDebug
Suppress=Init
suppress=MapVote
Suppress=VoiceChat
Suppress=ChatManager
Suppress=Timer
Paths=../Textures/Old2k4/*.utx
Paths=../Sounds/Old2k4/*.uax
Paths=../Music/Old2k4/*.umx
Paths=../StaticMeshes/Old2k4/*.usx
Paths=../Animations/Old2k4/*.ukx
Paths=../KarmaData/Old2k4/*.ka

[Engine.GameEngine]
CacheSizeMegs=32
UseSound=True
VoIPAllowVAD=False
ServerActors=IpDrv.MasterServerUplink
ServerActors=UWeb.WebServer
ServerPackages=Core
ServerPackages=Engine
ServerPackages=Fire
ServerPackages=Editor
ServerPackages=IpDrv
ServerPackages=UWeb
ServerPackages=GamePlay
ServerPackages=UnrealGame
ServerPackages=XGame
ServerPackages=XInterface
ServerPackages=GUI2K4
ServerPackages=xVoting
UseStaticMeshBatching=True
ColorHighDetailMeshes=False
ColorSlowCollisionMeshes=False
ColorNoCollisionMeshes=False
ColorWorldTextures=False
ColorPlayerAndWeaponTextures=False
ColorInterfaceTextures=False
MainMenuClass=KFGUI.KFMainMenu
ConnectingMenuClass=GUI2K4.UT2K4ServerLoading
DisconnectMenuClass=KFGUI.KFDisconnectPage
SinglePlayerMenuClass=KFGUI.KFMainMenu
InstantActionMenuClass=KFGUI.KFMainMenu
LoadingClass=ROInterface.ROServerLoading
ServerPackages=ROEffects
ServerPackages=ROEngine
ServerPackages=ROInterface
ServerPackages=KFMod
ServerPackages=KFChar

[WinDrv.WindowsClient]
WindowedViewportX=1920
WindowedViewportY=1080
FullscreenViewportX=1920
FullscreenViewportY=1080
MenuViewportX=640
MenuViewportY=480
Brightness=0.800000
Contrast=0.700000
Gamma=0.800000
UseJoystick=False
CaptureMouse=True
StartupFullscreen=True
ScreenFlashes=True
NoLighting=False
MinDesiredFrameRate=35.000000
AnimMeshDynamicLOD=0.000000
Decals=True
Coronas=True
DecoLayers=True
Projectors=True
NoDynamicLights=False
ReportDynamicUploads=False
TextureDetailInterface=Higher
TextureDetailTerrain=Higher
TextureDetailWeaponSkin=Higher
TextureDetailPlayerSkin=Higher
TextureDetailWorld=Higher
TextureDetailRenderMap=Higher
TextureDetailLightmap=Higher
NoFractalAnim=False
ScaleHUDX=0.000000
MouseXMultiplier=1.000000
MouseYMultiplier=1.000000
UseSpeechRecognition=True
WeatherEffects=True
DrawDistanceLOD=1.000000
Bloom=False
DisplaySettingsVer=1

[SDLDrv.SDLClient]
WindowedViewportX=800
WindowedViewportY=600
FullscreenViewportX=800
FullscreenViewportY=600
MenuViewportX=640
MenuViewportY=480
Brightness=0.800000
Contrast=0.700000
Gamma=0.800000
UseJoystick=False
JoystickNumber=0
IgnoreHat=False
JoystickHatNumber=0
CaptureMouse=True
StartupFullscreen=True
ScreenFlashes=True
NoLighting=False
MinDesiredFrameRate=35.000000
AnimMeshDynamicLOD=0.0
Decals=True
Coronas=True
DecoLayers=True
Projectors=True
NoDynamicLights=False
ReportDynamicUploads=False
TextureDetailInterface=Normal
TextureDetailTerrain=Normal
TextureDetailWeaponSkin=Normal
TextureDetailPlayerSkin=Normal
TextureDetailWorld=Normal
TextureDetailRenderMap=Normal
TextureDetailLightmap=UltraHigh
TextureMaxLOD=0
TextureMinLOD=0
NoFractalAnim=False
WeatherEffects=True
DrawDistanceLOD=1.0
IgnoreUngrabbedMouse=False
AllowUnicodeKeys=False
AllowCommandQKeys=True
MacFakeMouseButtons=True
MacKeepAllScreensOn=False
TextToSpeechFile=
MacNativeTextToSpeech=True

[ALAudio.ALAudioSubsystem]
UseEAX=False
Use3DSound=False
UseDefaultDriver=True
CompatibilityMode=False
MaxEAXVersion=255
UsePrecache=True
ReverseStereo=False
Channels=32
MusicVolume=0.10000
AmbientVolume=0.500000
SoundVolume=0.30000
VoiceVolume=10.000000
VolumeScaleRec=0.100000
DopplerFactor=1.0
Rolloff=0.5
TimeBetweenHWUpdates=15
DisablePitch=False
LowQualitySound=False
UseVoIP=True
UseVAD=False
UseSpatializedVoice=False
SpatializedVoiceRadius=100000
EnhancedDenoiser=False
LocalZOffset=0.0
DampenWithVoIP=False

[IpDrv.TcpNetDriver]
AllowDownloads=True
ConnectionTimeout=30.0
InitialConnectTimeout=200.0
AckTimeout=1.0
KeepAliveTime=0.2
MaxClientRate=15000
MaxInternetClientRate=10000
SimLatency=0
RelevantTimeout=5.0
SpawnPrioritySeconds=1.0
ServerTravelPause=4.0
NetServerMaxTickRate=30
LanServerMaxTickRate=35
DownloadManagers=IpDrv.HTTPDownload
DownloadManagers=Engine.ChannelDownload
AllowPlayerPortUnreach=False
LogPortUnreach=False
MaxConnPerIPPerMinute=5
LogMaxConnPerIPPerMin=False

[IpServer.UdpServerQuery]
GameName=rom

[IpDrv.MasterServerUplink]
DoUplink=True
UplinkToGamespy=True
SendStats=False
ServerBehindNAT=False
DoLANBroadcast=True

[IpDrv.MasterServerLink]
LANPort=11707
LANServerPort=10707
MasterServerList=(Address=""207.135.144.10"",Port=28902)
MasterServerList=(Address=""207.135.144.11"",Port=28902)

[IpDrv.HTTPDownload]
RedirectToURL=
ProxyServerHost=
ProxyServerPort=3128
UseCompression=True

[Engine.DemoRecDriver]
AllowDownloads=True
DemoSpectatorClass=UnrealGame.DemoRecSpectator
MaxClientRate=25000
ConnectionTimeout=15.0
InitialConnectTimeout=200.0
AckTimeout=1.0
KeepAliveTime=1.0
SimLatency=0
RelevantTimeout=5.0
SpawnPrioritySeconds=1.0
ServerTravelPause=4.0
NetServerMaxTickRate=30
LanServerMaxTickRate=30

[Engine.GameReplicationInfo]
ServerName=Killing Floor Server
ShortName=KF Server
ServerRegion=0
AdminName=
AdminEmail=
MessageOfTheDay=

[D3DDrv.D3DRenderDevice]
DetailTextures=True
HighDetailActors=True
SuperHighDetailActors=True
UsePrecaching=True
UseTrilinear=True
AdapterNumber=-1
ReduceMouseLag=True
UseTripleBuffering=False
UseHardwareTL=True
UseHardwareVS=True
UseCubemaps=True
DesiredRefreshRate=60
UseCompressedLightmaps=True
UseStencil=False
Use16bit=False
Use16bitTextures=False
MaxPixelShaderVersion=255
UseVSync=True
LevelOfAnisotropy=1
DetailTexMipBias=0.0
DefaultTexMipBias=-0.5
UseNPatches=False
TesselationFactor=1.0
CheckForOverflow=False
AvoidHitches=False
OverrideDesktopRefreshRate=False
ReportUnusedTextures=False

[D3D9Drv.D3D9RenderDevice]
DetailTextures=True
HighDetailActors=True
SuperHighDetailActors=True
UsePrecaching=True
UseTrilinear=True
AdapterNumber=-1
ReduceMouseLag=True
UseTripleBuffering=False
UseHardwareTL=True
UseHardwareVS=True
UseCubemaps=True
DesiredRefreshRate=60
UseCompressedLightmaps=True
UseStencil=False
Use16bit=False
Use16bitTextures=False
MaxPixelShaderVersion=255
UseVSync=False
LevelOfAnisotropy=1
DetailTexMipBias=0.0
DefaultTexMipBias=-0.5
UseNPatches=False
TesselationFactor=1.0
CheckForOverflow=False
OverrideDesktopRefreshRate=False

[OpenGLDrv.OpenGLRenderDevice]
DetailTextures=True
HighDetailActors=True
SuperHighDetailActors=True
UsePrecaching=True
UseCompressedLightmaps=True
UseTrilinear=True
UseStencil=False
MaxTextureUnits=8
VARSize=32
ReduceMouseLag=True
UseVSync=False
LevelOfAnisotropy=1.0
DetailTexMipBias=0.0
DefaultTexMipBias=-0.5
UseVBO=False
UseVSync=False
AppleVA=1
MultisampleBuffers=0
MultisampleSamples=0
MultisampleHint=2

[PixoDrv.PixoRenderDevice]
FogEnabled=True
Zoom2X=True
SimpleMaterials=True
LimitTextureSize=True
LowQualityTerrain=True
TerrainLOD=10
SkyboxHack=True
FilterQuality3D=1
FilterQualityHUD=1
HighDetailActors=False
SuperHighDetailActors=False
ReduceMouseLag=False
DesiredRefreshRate=0
DetailTexMipBias=0.000000
Use16bitTextures=False
Use16bit=True
UseStencil=False
UseCompressedLightmaps=False
DetailTextures=False
UsePrecaching=True

[Engine.NullRenderDevice]
DetailTextures=True
HighDetailActors=True
SuperHighDetailActors=True
UsePrecaching=True
UseCompressedLightmaps=True
UseStencil=False

[Editor.EditorEngine]
UseSound=True
CacheSizeMegs=32
GridEnabled=True
SnapVertices=False
SnapDistance=1.000000
GridSize=(X=4.000000,Y=4.000000,Z=4.000000)
RotGridEnabled=True
RotGridSize=(Pitch=1024,Yaw=1024,Roll=1024)
GameCommandLine=-log
FovAngleDegrees=90.000000
GodMode=True
AutoSave=True
AutoSaveTimeMinutes=5
AutoSaveIndex=6
UseAxisIndicator=True
MatineeCurveDetail=0.1
ShowIntWarnings=False
UseSizingBox=True
RenderDevice=D3DDrv.D3DRenderDevice
AudioDevice=ALAudio.ALAudioSubsystem
NetworkDevice=IpDrv.TcpNetDriver
DemoRecordingDevice=Engine.DemoRecDriver
Console=Engine.Console
Language=ute
AlwaysShowTerrain=False
UseActorRotationGizmo=False
LoadEntirePackageWhenSaving=0
EditPackages=Core
EditPackages=Engine
EditPackages=Fire
EditPackages=Editor
EditPackages=UnrealEd
EditPackages=IpDrv
EditPackages=UWeb
EditPackages=GamePlay
EditPackages=UnrealGame
EditPackages=XGame
EditPackages=XInterface
EditPackages=XAdmin
EditPackages=XWebAdmin
EditPackages=GUI2K4
EditPackages=xVoting
EditPackages=UTV2004c
EditPackages=UTV2004s
EditPackages=ROEffects
EditPackages=ROEngine
EditPackages=ROInterface
EditPackages=Old2k4
EditPackages=KFMod
EditPackages=KFChar
EditPackages=KFGui
EditPackages=GoodKarma
EditPackages=KFMutators
EditPackages=KFStoryGame
EditPackages=KFStoryUI
EditPackages=SideShowScript
EditPackages=FrightScript
CutdownPackages=Core
CutdownPackages=Editor
CutdownPackages=Engine
CutdownPackages=Fire
CutdownPackages=GamePlay
CutdownPackages=GUI2K4
CutdownPackages=IpDrv
CutdownPackages=Onslaught
CutdownPackages=UnrealEd
CutdownPackages=UnrealGame
CutdownPackages=UWeb
CutdownPackages=XAdmin
CutdownPackages=XEffects
CutdownPackages=XInterface
CutdownPackages=XPickups
CutdownPackages=XWebAdmin
CutdownPackages=XVoting

[UWeb.WebServer]
Applications[0]=xWebAdmin.UTServerAdmin
ApplicationPaths[0]=/ServerAdmin
Applications[1]=xWebAdmin.UTImageServer
ApplicationPaths[1]=/images
bEnabled=False
ListenPort=8075

[Engine.Console]
ConsoleHotKey=9
TimePerTitle=30.0
TimePerDemo=60.0
TimePerSoak=3600.0
TimeTooIdle=60.0
DemoLevels[0]=DM-Curse3
DemoLevels[1]=DM-Antalus
DemoLevels[2]=CTF-Chrome
DemoLevels[3]=DOM-SunTemple
DemoLevels[4]=BR-Endagra

[Engine.AccessControl]
AdminPassword=
GamePassword=
bBanByID=True

[Engine.GameInfo]
GoreLevel=2
MaxSpectators=0
MaxPlayers=6
AutoAim=1.000000
GameSpeed=1.000000
bChangeLevels=True
bStartUpLocked=False
bNoBots=True
bAttractAlwaysFirstPerson=False
NumMusicFiles=13
HUDType=Engine.Hud
MaxLives=0
TimeLimit=0
GoalScore=0
GameStatsClass=IpDrv.MasterServerGameStats
SecurityClass=UnrealGame.UnrealSecurity
AccessControlClass=Engine.AccessControl
VotingHandlerType=xVoting.xVotingHandler
MaxIdleTime=+0.0

[Engine.AmbientSound]
AmbientVolume=0.25

[Engine.LevelInfo]
PhysicsDetailLevel=PDL_Medium
MeshLODDetailLevel=MDL_Ultra
bLowSoundDetail=False
DecalStayScale=1.0
bNeverPrecache=false
TimeMarginSlack=1.35
MaxClientFrameRate=+90.0
bShouldPreload=True
bDesireSkinPreload=True

[XInterface.ExtendedConsole]
ConsoleHotKey=192
NeedPasswordMenuClass=GUI2K4.UT2K4GetPassword
bSpeechMenuUseMouseWheel=True
bSpeechMenuUseLetters=False
SMOriginX=0.01
SMOriginY=0.3
LetterKeys[0]=IK_Q
LetterKeys[1]=IK_W
LetterKeys[2]=IK_E
LetterKeys[3]=IK_R
LetterKeys[4]=IK_A
LetterKeys[5]=IK_S
LetterKeys[6]=IK_D
LetterKeys[7]=IK_F
LetterKeys[8]=IK_Z
LetterKeys[9]=IK_X
MusicManagerClassName=GUI2K4.StreamPlayer

[XGame.xDeathMatch]
HUDType=XInterface.HudBDeathMatch
MaxLives=0
TimeLimit=20
GoalScore=25
bTeamScoreRound=False
bPlayersMustBeReady=False
bAllowTaunts=True
bForceRespawn=False
bWeaponStay=true

[XGame.xTeamGame]
HUDType=XInterface.HudBTeamDeathMatch
MaxLives=0
TimeLimit=20
GoalScore=60
bTeamScoreRound=False
bPlayersMustBeReady=False
bAllowTaunts=True
FriendlyFireScale=0
MaxTeamSize=16
bForceRespawn=False
bWeaponStay=true

[XGame.xCTFGame]
HUDType=XInterface.HudBCaptureTheFlag
MaxLives=0
TimeLimit=20
GoalScore=3
bTeamScoreRound=False
bPlayersMustBeReady=False
bAllowTaunts=True
FriendlyFireScale=0
MaxTeamSize=16
bForceRespawn=False
bWeaponStay=true

[XGame.xDoubleDom]
HUDType=XInterface.HudBDoubleDomination
MaxLives=0
TimeLimit=20
GoalScore=3
bTeamScoreRound=False
bPlayersMustBeReady=False
bAllowTaunts=True
TimeToScore=10
TimeDisabled=10
FriendlyFireScale=0
MaxTeamSize=16
bForceRespawn=False
bWeaponStay=true

[XGame.xBombingRun]
HUDType=XInterface.HudBBombingRun
MaxLives=0
TimeLimit=20
GoalScore=15
bTeamScoreRound=False
bPlayersMustBeReady=False
bAllowTaunts=True
FriendlyFireScale=0
MaxTeamSize=16
bForceRespawn=False
bWeaponStay=true

[xVoting.xVotingHandler]
VoteTimeLimit=30
bKickVote=True
RepeatLimit=1
KickPercent=50

[Engine.MaplistManager]
Games=(GameType=""BonusPack.xLastManStandingGame"",ActiveMaplist=""Default LMS"")
Games=(GameType=""BonusPack.xMutantGame"",ActiveMaplist=""Default MUT"")
Games=(GameType=""Onslaught.ONSOnslaughtGame"",ActiveMaplist=""Default ONS"")
Games=(GameType=""SkaarjPack.Invasion"",ActiveMaplist=""Default INV"")
Games=(GameType=""UT2k4Assault.ASGameInfo"",ActiveMaplist=""Default AS"")
Games=(GameType=""XGame.xBombingRun"",ActiveMaplist=""Default BR"")
Games=(GameType=""XGame.xCTFGame"",ActiveMaplist=""Default CTF"")
Games=(GameType=""XGame.xDeathMatch"",ActiveMaplist=""Default DM"")
Games=(GameType=""XGame.xDoubleDom"",ActiveMaplist=""Default DOM2"")
Games=(GameType=""XGame.xTeamGame"",ActiveMaplist=""Default TDM"")
Games=(GameType=""XGame.xVehicleCTFGame"",ActiveMaplist=)
Games=(GameType=""ROEngine.ROTeamGame"",ActiveMaplist=""Default RO"")
Games=(GameType=""KFmod.KFGameType"",ActiveMaplist=""Default KF"")
Games=(GameType=""KFmod.KFGameTypePvP"",ActiveMaplist=""Default KFP"")
Games=(GameType=""KFStoryGame.KFstoryGameInfo"",ActiveMapList=""Default KFO"")

[XInterface.MapListDeathMatch]
MapNum=0
Maps=DM-RRAJIGAR
Maps=DM-RANKIN
Maps=DM-CORRUGATION
Maps=DM-DE-GRENDELKEEP
Maps=DM-DE-IRONIC
Maps=DM-DE-OSIRIS2
Maps=DM-GESTALT
Maps=DM-IRONDEITY
Maps=DM-METALLURGY
Maps=DM-Deck17
Maps=DM-Antalus
Maps=DM-Asbestos
Maps=DM-Curse4

[XInterface.MapListTeamDeathMatch]
MapNum=0
Maps=DM-RRAJIGAR
Maps=DM-RANKIN
Maps=DM-CORRUGATION
Maps=DM-DE-GRENDELKEEP
Maps=DM-DE-IRONIC
Maps=DM-DE-OSIRIS2
Maps=DM-GESTALT
Maps=DM-IRONDEITY
Maps=DM-METALLURGY
Maps=DM-Deck17
Maps=DM-Antalus
Maps=DM-Asbestos
Maps=DM-Curse4

[XInterface.MapListCaptureTheFlag]
MapNum=0
Maps=CTF-ABSOLUTEZERO
Maps=CTF-MOONDRAGON
Maps=CTF-GRASSYKNOLL
Maps=CTF-COLOSSUS
Maps=CTF-SMOTE
Maps=CTF-DOUBLEDAMMAGE
Maps=CTF-AVARIS
Maps=CTF-BRIDGEOFFATE
Maps=CTF-FaceClassic
Maps=CTF-CHROME
Maps=CTF-Citadel
Maps=CTF-Orbital2

[DefaultRO MaplistRecord]
DefaultTitle=Default RO
DefaultGameType=ROEngine.ROTeamGame
DefaultActive=3
DefaultMaps=RO-Danzig
DefaultMaps=RO-Arad
DefaultMaps=RO-Barashka
DefaultMaps=RO-Basovka
DefaultMaps=RO-Bondarevo
DefaultMaps=RO-HedgeHog
DefaultMaps=RO-Kaukasus
DefaultMaps=RO-KrasnyiOktyabr
DefaultMaps=RO-Ogledow
DefaultMaps=RO-Odessa
DefaultMaps=RO-StalingradKessel
DefaultMaps=RO-KonigsPlatz
DefaultMaps=RO-Rakowice
DefaultMaps=RO-BaksanValley
DefaultMaps=RO-Berezina
DefaultMaps=RO-BlackDayJuly
DefaultMaps=RO-Kryukovo
DefaultMaps=RO-KurlandKessel
DefaultMaps=RO-Leningrad
DefaultMaps=RO-Mannikkala
DefaultMaps=RO-SmolenskStalemate
DefaultMaps=RO-Tcherkassy
DefaultMaps=RO-TulaOutskirts
DefaultMaps=RO-Zhitomir1941

[ROInterface.ROMapList]
MapNum=0
Maps=RO-Danzig
Maps=RO-Arad
Maps=RO-Barashka
Maps=RO-Basovka
Maps=RO-Bondarevo
Maps=RO-HedgeHog
Maps=RO-Kaukasus
Maps=RO-KrasnyiOktyabr
Maps=RO-Ogledow
Maps=RO-Odessa
Maps=RO-StalingradKessel
Maps=RO-KonigsPlatz
Maps=RO-Rakowice
Maps=RO-BaksanValley
Maps=RO-Berezina
Maps=RO-BlackDayJuly
Maps=RO-Kryukovo
Maps=RO-KurlandKessel
Maps=RO-Leningrad
Maps=RO-Mannikkala
Maps=RO-SmolenskStalemate
Maps=RO-Tcherkassy
Maps=RO-TulaOutskirts
Maps=RO-Zhitomir1941

[ROEngine.ROTeamGame]
WinLimit=2
RoundLimit=3
PreStartTime=5

[ROEngine.ROWeaponAttachment]
WeaponAmbientScale=5.0

[UnrealGame.DeathMatch]
RestartWait=10

[ROFirstRun]
ROFirstRun=1094

";
        public const string UserIni = @"[DefaultPlayer]
Name=KFPlayer
Class=Engine.Pawn
Character=Corporal_Lewis
team=1
Sex=M

[Engine.Input]
Aliases[0]=(Command=""Button bFire | Fire"",Alias=Fire)
Aliases[1]=(Command=""Button bAltFire | AltFire"",Alias=AltFire)
Aliases[2]=(Command=""Axis aBaseY  Speed=+300.0"",Alias=MoveForward)
Aliases[3]=(Command=""Axis aBaseY  Speed=-300.0"",Alias=MoveBackward)
Aliases[4]=(Command=""Axis aBaseX Speed=-40.0"",Alias=TurnLeft)
Aliases[5]=(Command=""Axis aBaseX  Speed=+40.0"",Alias=TurnRight)
Aliases[6]=(Command=""Axis aStrafe Speed=-300.0"",Alias=StrafeLeft)
Aliases[7]=(Command=""Axis aStrafe Speed=+300.0"",Alias=StrafeRight)
Aliases[8]=(Command=""Jump | Axis aUp Speed=+300.0"",Alias=Jump)
Aliases[9]=(Command=""Crouch | Axis aUp Speed=-300.0| OnRelease UnCrouch"",Alias=""Duck"")
Aliases[10]=(Command=""Button bLook"",Alias=Look)
Aliases[11]=(Command=""Toggle bLook"",Alias=LookToggle)
Aliases[12]=(Command=""ActivateItem"",Alias=InventoryActivate)
Aliases[13]=(Command=""NextItem"",Alias=InventoryNext)
Aliases[14]=(Command=""PrevItem"",Alias=InventoryPrevious)
Aliases[15]=(Command=""Axis aLookUp Speed=+25.0"",Alias=LookUp)
Aliases[16]=(Command=""Axis aLookUp Speed=-25.0"",Alias=LookDown)
Aliases[17]=(Command=""Button bSnapLevel"",Alias=CenterView)
Aliases[18]=(Command=""Button bRun"",Alias=Walking)
Aliases[19]=(Command=""Button bStrafe"",Alias=Strafe)
Aliases[20]=(Command=""NextWeapon"",Alias=NextWeapon)
Aliases[21]=(Command=""Button bFreeLook"",Alias=FreeLook)
Aliases[22]=(Command=""ViewClass Pawn"",Alias=ViewTeam)
Aliases[23]=(Command=""Button bTurnToNearest"",Alias=TurnToNearest)
Aliases[24]=(Command=""Button bTurn180"",Alias=Turn180)
Aliases[25]=(Command=""ShowScores|OnRelease HideScores"",Alias=ScoreToggle)
Aliases[26]=(Command=""InGameChat"",Alias=InGameChat)
Aliases[27]=(Command=""ServerInfo"",Alias=ServerInfo)
Aliases[28]=(Command=""Button bVoiceTalk"",Alias=VoiceTalk)
Aliases[29]=(Command=""Speak Team|Button bVoiceTalk|OnRelease SpeakLast"",Alias=ToggleTeamChat)
Aliases[30]=(Command=""Speak Local|Button bVoiceTalk|OnRelease SpeakLast"",Alias=ToggleLocalChat)
Aliases[31]=(Command=""Speak Public|Button bVoiceTalk|OnRelease SpeakLast"",Alias=TogglePublicChat)
Aliases[32]=(Command=""Axis aBaseX SpeedBase=100.0 DeadZone=0.1"",Alias=""SpaceFighter_JoyX"")
Aliases[33]=(Command=""Axis aLookUp SpeedBase=100.0 DeadZone=0.1"",Alias=""SpaceFighter_JoyY"")
Aliases[34]=(Command=""Axis aUp Speed=+300.0 | Axis aStrafe SpeedBase=300.0 DeadZone=0.1"",Alias=""SpaceFighter_JoyV"")
Aliases[35]=(Command=""Axis aBaseY SpeedBase=300.0 DeadZone=0.1 Invert=-1"",Alias=""SpaceFighter_JoySlider1"")
Aliases[36]=(Command=""IronSightZoomIn | onrelease IronSightZoomOut"",Alias=""Aiming"")
Aliases[37]=(Command=""ToggleIronSights"",Alias=""ToggleAiming"")
Aliases[38]=(Command=""ReloadMeNow"",Alias=""ReloadWeapon"")
Aliases[39]=(Command=""ThrowGrenade"",Alias=""ThrowNade"")
Aliases[40]=(Command=""QuickHeal"",Alias=""QuickHeal"")
Aliases[41]=(Command=""Use | OnRelease StopUsing"",Alias=""Use"")
0=
1=SwitchWeapon 1
2=SwitchWeapon 2
3=SwitchWeapon 3
4=SwitchWeapon 4
5=SwitchWeapon 5
6=
7=
8=
9=
F1=ShowScores
F2=ToggleConditionStack
F3=
F4=ToggleBehindView
F5=ToggleFreeCam
F6=Stat Net
F7=
F8=
F9=shot
F10=Cancel
F11=
F12=
F13=
F14=
F15=
F16=
F17=
F18=
F19=
F20=
F21=
F22=
F23=
F24=
A=StrafeLeft
B=TossCash
C=ToggleDuck
D=StrafeRight
E=Use
F=ToggleFlashlight
G=ThrowNade
H=
I=ToggleAiming
j=
K=ShowKickMenu
L=
M=
N=
O=
P=
Q=QuickHeal
R=ReloadWeapon
S=MoveBackward
T=Talk
U=
V=SpeechMenuToggle
W=MoveForward
X=
Y=TeamTalk
Z=Use
Alt=
Attn=
Backslash=ThrowWeapon
Backspace=Jump
Cancel=
CapsLock=VoiceTalk
Comma=StrafeLeft
CrSel=
Ctrl=Walking
Delete=Speak Local
Down=MoveBackward
End=Speak Team
Enter=
Equals=GrowHUD
Escape=ShowMenu
ErEof=
Execute=
ExSel=
GreySlash=
GreyStar=ShowVoteMenu
GreyMinus=
GreyPlus=
Help=
home=Speak Public
Insert=WriteToLog
LControl=
Left=StrafeLeft
LeftBracket=
LShift=
Minus=ShrinkHUD
NoName=
None=
OemClear=
PA1=
PageDown=LookDown
PageUp=
Pause=Pause
Period=StrafeRight
Play=
Print=
PrintScrn=
RControl=
Right=StrafeRight
RightBracket=
RShift=
ScrollLock=
Select=
Semicolon=
Separator=
Shift=
SingleQuote=Strafe
Slash=NextWeapon
Space=Jump
Tab=ScoreToggle
Tilde=ConsoleToggle
Up=MoveForward
Zoom=
LeftMouse=Fire
MiddleMouse=AltFire
RightMouse=ToggleAiming
MouseWheelDown=PrevWeapon
MouseWheelUp=NextWeapon
MouseX=Count bXAxis | Axis aMouseX Speed=2.0
MouseY=Count bYAxis | Axis aMouseY Speed=2.0
MouseZ=
NumLock=
NumPad6=
NumPad5=
NumPad4=
NumPad3=
NumPad2=
NumPad1=
NumPad0=
NumPad7=
NumPad8=
NumPad9=
NumPad9=StrafeRight
NumPadPeriod=
Joy1=Fire
Joy2=Jump
Joy3=AltFire
Joy4=Duck
Joy5=NextWeapon
Joy6=SwitchWeapon 2
Joy7=SwitchWeapon 3
Joy8=SwitchWeapon 4
Joy9=SwitchWeapon 9
Joy10=SwitchWeapon 0
Joy11=InventoryPrevious
Joy12=InventoryActivate
Joy13=MoveForward
Joy14=StrafeRight
Joy15=MoveBackward
Joy16=StrafeLeft
JoyX=Axis aStrafe SpeedBase=300.0 DeadZone=0.1
JoyY=Axis aBaseY SpeedBase=300.0 DeadZone=0.1 Invert=-1
JoyZ=
JoyR=
JoyU=
JoyV=Axis aBaseX SpeedBase=2.0 DeadZone=0.4
JoySlider1=Axis aLookUp SpeedBase=2.0 DeadZone=0.4
JoySlider2=

[Engine.Controller]
Handedness=1.000000

[Engine.PlayerController]
bNeverSwitchOnPickup=false
bNoVoiceMessages=false
bNoTextToSpeechVoiceMessages=true
bOnlySpeakTeamText=false
TextToSpeechVoiceVolume=1.0
bNoVoiceTaunts=false
bNoAutoTaunts=false
bAutoTaunt=false
bNoMatureLanguage=false
AnnouncerVolume=4
AnnouncerLevel=2
DesiredFOV=85.000000
DefaultFOV=85.000000
FOVAngle=90.0
bLookUpStairs=False
bSnapToLevel=False
bAlwaysMouseLook=True
bKeyboardLook=True
bAlwaysLevel=False
ngSecretSet=False
EnemyTurnSpeed=45000
InputClass=Class'Engine.PlayerInput'
TeamBeaconMaxDist=4000.000000
TeamBeaconPlayerInfoMaxDist=1200.000000
TeamBeaconTexture=TeamSymbols.TeamBeaconT
TeamBeaconTeamColors[0]=(B=0,G=0,R=180,A=255)
TeamBeaconTeamColors[1]=(B=200,G=80,R=80,A=255)
TeamBeaconCustomColor=(B=0,G=255,R=255,A=255)
TeamBeaconUseCustomColor=True
bSmallWeapons=true
bEnableWeaponForceFeedback=False
bEnablePickupForceFeedback=False
bEnableDamageForceFeedback=False
bEnableGUIForceFeedback=False
bDynamicNetSpeed=False
bLandingShake=true
DemoMenuClass=GUI2K4.UT2K4DemoPlayback
AutoJoinMask=5
bEnableInitialChatRoom=True
MidGameMenuClass=ROInterface.RODisconnectOptionPage
;AdminMenuClass=GUI2K4.RemoteAdmin
ChatPasswordMenuClass=GUI2K4.UT2K4ChatPassword
VoiceChatCodec=CODEC_48NB
VoiceChatLANCodec=CODEC_96WB

[Engine.Pawn]
Bob=0.006
bWeaponBob=True

[Engine.Vehicle]
bVehicleShadows=True

[Engine.Player]
ConfiguredInternetSpeed=9636
ConfiguredLanSpeed=20000

[Engine.HUD]
bSmallWeaponBar=true
bHideHUD=false
HudOpacity=255
HudScale=1.0
HudCanvasScale=1.0
bMessageBeep=true
bShowWeaponInfo=true
bShowWeaponBar=True
bShowPersonalInfo=true
bShowPoints=true
bCrosshairShow=false
bShowPortrait=True
bNoEnemyNames=False
CrosshairScale=1.0
CrosshairOpacity=1.0
CrosshairStyle=0
ConsoleMessageCount=4
ConsoleFontSize=5
MessageFontOffset=0

[XGame.xDeathMessage]
bNoConsoleDeathMessages=False

[XInterface.GUIController]
MenuMouseSens=1.25
bModAuthor=false
bExpert=false

[GUI2K4.UT2K4GUIController]
MenuMouseSens=1.250000

[GUI2K4.SettingsTabs]
bExpert=False

[GUI2K4.UT2K4Browser_ServerListBox]
FiltersPage=GUI2K4.SimpleFilterPage

[GUI2K4.UT2K4ServerLoading]
Backgrounds=2k4Menus.Loading.loadingscreen1
Backgrounds=2k4Menus.Loading.loadingscreen2
Backgrounds=2k4Menus.Loading.loadingscreen2
Backgrounds=2k4Menus.Loading.loadingscreen4

[DemoRecording]
DemoMask=Demo%td

[Screenshots]
ShotMask=Shot%c
ShotCount=0
ShotDir=..\Screenshots

[Engine.TextToSpeechAlias]
RemoveCharacters=|:][}{^/\~()*
Aliases=(MatchWords=(""gg""),ReplaceWord=""good game"")
Aliases=(MatchWords=(""rofl"",""rotfl"",""rotflmao""),ReplaceWord=""rolls on floor laughing!"")
Aliases=(MatchWords=(""lol""),ReplaceWord=""laughing out loud!"")
Aliases=(MatchWords=(""thx""),ReplaceWord=""thanks"")
Aliases=(MatchWords=(""np""),ReplaceWord=""no problem"")
Aliases=(MatchWords=("":)"","":-)"","":P""),ReplaceWord=""smile"")
Aliases=(MatchWords=("";)"","";-)"","";P""),ReplaceWord=""wink"")
Aliases=(MatchWords=(""omg"",""omfg""),ReplaceWord=""oh my god!"")
Aliases=(MatchWords=(""ns""),ReplaceWord=""nice shot"")
Aliases=(MatchWords=(""hf""),ReplaceWord=""have fun"")
Aliases=(MatchWords=(""fc""),ReplaceWord=""flag carrier"")
Aliases=(MatchWords=(""ih""),ReplaceWord=""incoming high"")
Aliases=(MatchWords=(""iw""),ReplaceWord=""incoming low"")
Aliases=(MatchWords=(""ir""),ReplaceWord=""incoming right"")
Aliases=(MatchWords=(""il""),ReplaceWord=""incoming left"")
Aliases=(MatchWords=(""thx""),ReplaceWord=""thanks"")
Aliases=(MatchWords=(""gl""),ReplaceWord=""good luck"")
Aliases=(MatchWords=(""cya""),ReplaceWord=""seeya"")
Aliases=(MatchWords=(""gj""),ReplaceWord=""good job"")
Aliases=(MatchWords=(""ty""),ReplaceWord=""thank you"")
Aliases=(MatchWords=(""bbl""),ReplaceWord=""be back later"")
Aliases=(MatchWords=(""brb""),ReplaceWord=""be right back"")
Aliases=(MatchWords=(""bbiab""),ReplaceWord=""be back in a bit"")
Aliases=(MatchWords=(""woot"",""w00t""),ReplaceWord=""woute"")
Aliases=(MatchWords=(""woot!"",""w00t!""),ReplaceWord=""woute!"")
Aliases=(MatchWords=(""woohoo""),ReplaceWord=""woo who"")

[UnrealGame.UnrealPlayer]
CustomStatusAnnouncerPack=
CustomRewardAnnouncerPack=

[GUI2K4.UT2K4IRC_Page]
bIRCTextToSpeechEnabled=False

[KFMOD.KFHintManager]
bUsedUpHints[0]=0
bUsedUpHints[1]=0
bUsedUpHints[2]=0
bUsedUpHints[3]=0
bUsedUpHints[4]=0
bUsedUpHints[5]=0
bUsedUpHints[6]=0
bUsedUpHints[7]=0
bUsedUpHints[8]=0
bUsedUpHints[9]=0
bUsedUpHints[10]=0
bUsedUpHints[11]=0
bUsedUpHints[12]=0
bUsedUpHints[13]=0
bUsedUpHints[14]=0
bUsedUpHints[15]=0
bUsedUpHints[16]=0
bUsedUpHints[17]=0
bUsedUpHints[18]=0
bUsedUpHints[19]=0
bUsedUpHints[20]=0
bUsedUpHints[21]=0
bUsedUpHints[22]=0
bUsedUpHints[23]=0
bUsedUpHints[24]=0
bUsedUpHints[25]=0
bUsedUpHints[26]=0
bUsedUpHints[27]=0
bUsedUpHints[28]=0
bUsedUpHints[29]=0
bUsedUpHints[30]=0
bUsedUpHints[31]=0
bUsedUpHints[32]=0
bUsedUpHints[33]=0
bUsedUpHints[34]=0
bUsedUpHints[35]=0
bUsedUpHints[36]=0
bUsedUpHints[37]=0
bUsedUpHints[38]=0
bUsedUpHints[39]=0
bUsedUpHints[40]=0
bUsedUpHints[41]=0
bUsedUpHints[42]=0
bUsedUpHints[43]=0
bUsedUpHints[44]=0
bUsedUpHints[45]=0
bUsedUpHints[46]=0
bUsedUpHints[47]=0
bUsedUpHints[48]=0
bUsedUpHints[49]=0
bUsedUpHints[50]=0
bUsedUpHints[51]=0
bUsedUpHints[52]=0
bUsedUpHints[53]=0
bUsedUpHints[54]=0
bUsedUpHints[55]=0
bUsedUpHints[56]=0
bUsedUpHints[57]=0
bUsedUpHints[58]=0
bUsedUpHints[59]=0
bUsedUpHints[60]=0

[GUI2K4.UT2K4Tab_MainBase]
bOnlyShowOfficial=False
bOnlyShowCustom=False
MaplistEditorMenu=KFGUI.KFMaplistEditor

[ROEngine.ROPlayer]
GlobalDetailLevel=4

[KFGUI.KFMainMenu]
MenuSong=KFMenu
AcceptedEULA=True

[WindowPositions]
GameLog=(X=78,Y=78,XL=512,YL=256)

";
    }
}
