# Space Redirection for Infinite room
<p align="center">
  <img 
    width="80%"
    src="/Resources/teaser_thumbnail.png"
  >
</p>

가상 현실에서의 탐험 기법은 여러 가지로 갈래로 나뉘어서 연구되고 있고 그 중 하나가 사용자의 실제 보행이 있습니다. 그러나, 가상과 실제 공간 차이로 인해 단순한 보행으로는 안전상 위험할 수 있고 사용자가 가상 현실에 몰입할 수 없다는 큰 단점이 존재합니다. 따라서, 사용자가 최대한 실제 공간과 충돌이 일어나지 않도록 가상 현실에서의 사용자의 보행을 미세하게 조정하는 방식 등이 연구되고 있습니다.

이러한 맥락에서 출발하여, 본 프로젝트에서는 무수히 많은 방들로 구성된 가상 실내 환경을 change blindness 현상을 이용한 벽 이동을 통해 실제 환경에서의 충돌 없이 사용자가 걸어서 탐험하는 일반화된 알고리즘을 제시하였습니다. 구체적으로 해당 기법은 사용자가 보지 않고 있는 동안 방 안의 벽면을 이동시켜 방을 scale하고 translation 하여, 현재 사용자를 포함하고 있는 가상의 방이 유효한 실제 공간 내에 항상 존재하도록 만듭니다. 따라서, 기존 방법들과 다르게 이론상 실제 공간의 충돌이 전혀 발생하지 않는다는 장점을 가집니다.

또한, 해당 프로젝트에서는 제안한 방법을 뒷받침 하기 위한 실험용 애플리케이션을 제작하였습니다. 크게 두 가지 실험을 진행하였고, 첫번째 실험은 시야 밖의 벽면이 어느 정도까지 이동해야 사용자가 이를 인지하는지 확인하는 detection threshold 실험이고, 두번째 실험은 제안한 방법과 기존의 다른 탐험 기법(Telport, Steer-to-Center)을 이용했을 때 사용자가 느끼는 사용성, 현존감 그리고 몰입감의 차이가 있는지를 확인하는 실험입니다.

# Requirements
- Unity 2019.4
- Unity Hub 2.4+

# How to Use
1. Clone 한 뒤에, Unity Hub를 통해서 Clone된 디렉토리를 추가하여 Unity 프로젝트로 실행합니다.
2. Asset/Scene 에서 원하는 Scene 선택하여 진입한 뒤에, Play 버튼을 눌러서 실행합니다.

* Asset/Scene 에는 VR 보행 훈련, 첫번째 실험, VR 내 상호작용이나 Teleport 기능 훈련, 두번째 실험이 준비되어 있습니다.
* 기본 실행 옵션은 키보드 조작을 통해서 움직일 수 있는 non-VR 로 설정되어 있으며, VR을 사용할 경우 SteamVR로 호환되는 VR 기기 (Oculus Quest 2, HTC VIVE 등) 를 연결한 뒤에 사용하셔야 됩니다.

# Structure
Unity의 Component로서 Object에 부착할 수 있는 클래스는 Environment, BaseRedirector, BaseResetter, BaseTask 로 4가지 입니다.

## Environment Module
### RealSpace
직사각형 실제 공간을 나타내고 실제 유저의 위치와 방향을 나타내는 RealUser를 가집니다.
### Users
각각 non-VR과 VR 사용자를 나타내는 User를 가지며, 실행 환경에 따라서 하나로 고정됩니다. VR 사용자의 경우 SteamVR의 일부 기능(Hand, Interactable 등)을 사용합니다.   
User는 Head, Hand, Body를 구성요소로 가집니다. Head에 부착된 Camera로 가상 공간을 바라보고 Hand를 통해서 물체와 상호작용하며, Body로 움직입니다.   
User가 어떤 행위를 할 때마다 저장된 Callback 함수들을 실행하는 Observer 패턴 (Event)을 사용합니다.
### VirtualSpace
가상 공간을 나타내며 Room과 Door로 이루어져 있습니다. 내부적으로는 Room이 노드이고 Door가 엣지가 되는 그래프 형태로 구현됩니다.

## 2DGeometry Module
### Transform2D
Transform2D를 통해서 어떤 물체의 외곽선을 2차원 형태로 나타내고 이것의 위치, 회전, 스케일 값을 관리합니다.   
두 개의 Transform2D 사이의 관계(겹쳐있는지, 포함되는지 등)를 판단합니다.   
Circle2D, Rectangle2D는 Transform2D을 상속 받으며 각각 2차원 상에서 원과 직사각형을 나타냅니다.

## User Interface Module
### UIManager 
애플리케이션 실행 도중 사용되는 모든 User Interface들을 관리합니다.
### UICanvas
User Interface를 담을 수 있는 컨테이너이며 UIManager는 UICanvas 단위로 관리하고 추적합니다.
### UIBase
User Interface를 나타내는 최소 단위(Button, Text, Image 등)입니다.   
User Interface와 상호작용할 때마다 저장된 Callback 함수들을 실행하는 Observer 패턴 (Event)을 사용합니다. 

<p align="center">
  <img 
    width="100%"
    src="/Resources/class_diagram_1.png"
  >
</p>

## Redirector Module
시뮬레이션에 적용할 Redirection 기법을 결정합니다. 
### SpaceRedirector
공간을 변형시켜서 사용자가 실제 공간 바깥으로 나가지 않도록 왜곡하는 방식을 구현합니다.
### S2CRedirector
사용자가 실제 공간의 중심으로 유도되도록 사용자의 경로를 왜곡하는 방식을 구현합니다.

## Resetter Module
시뮬레이션에 적용할 Reset 기법을 결정합니다.   
BaseResetter는 BaseTask를 상속받으며, 사용자가 실제 공간 바깥으로 나갈 때마다 사용자를 안쪽으로 재조정하는 Task를 추가합니다.     
하위 객체로는 얼만큼 재조정 할지에 따라서 CenterTurnResetter, TwoOneTurnResetter 를 가지고 있습니다.

## Task Module
### FiniteStateMachine (FSM)
추상 기계 모델 중 하나인 Finite State Machine 을 구현합니다.   
유한 개의 State를 가지며 Input이 들어올때마다 특정 State로 전이하는 Transition으로 이루어집니다. 
### BaseTask
실험의 시작부터 끝까지의 진행 과정을 FSM 형태로 나타내고 관리합니다.   
사용자가 어떤 행위를 할 때마다 이것을 전달받아서 FSM의 Input으로 사용하여 Task를 진행시킵니다.


<p align="center">
  <img 
    width="100%"
    src="/Resources/class_diagram_2.png"
  >
</p>

# Algorithm
<p align="center">
  <img 
    width="100%"
    src="/Resources/environment.png"
  >
</p>

해당 알고리즘에서 실내 공간은 방들과 이들을 이어주는 복도로 구성되어 있으며, 복도 또한 하나의 방으로 본다면 실내 공간은 무수히 많은 방들의 집합이며 주어진 넓이 안에서 간단하면서도 효율적으로 실내 공간을 구성하기 위해 대부분의 방들은 직사각형 형태를 이룬다고 가정합니다. 그림으로는 Figure 2와 같습니다. 이를 수식으로 표현하면 실제 공간 $R$ 은 직사각형이고 가상 공간 $V$ 은 $n$ 개의 방으로 구성되고 각 방은 실제 공간 안에 완전히 포함될 수 있는 크기의 직사각형이라고 할 수 있습니다. 그리고 각 방은 인접한 다른 방과 문으로 연결될 수 있습니다.

<p align="center">
  <img 
    width="100%"
    src="/Resources/method.png"
  >
</p>

이런 가정 하에, 제안하는 알고리즘은 다음과 같습니다. 

1. Figure 3(a), 3(b), and line 6 in Algorithm 1에 나타낸 것처럼, 해당 방법은 우선 사용자가 처음 시작할 방을 실제 공간 중심으로 이동시키고 해당 방에 이웃한 방들을 실제 공간 안쪽으로 압축하여 초기화합니다. 
2. 이후에 이 방법은 사용자가 새로운 방에 방문할 때마다 복원 후 압축 단계를 진행시킵니다. (Figure 3(c) and line 9 in Algorithm 1). 
3. 먼저 Figure 3(d) 와 line 18,19 in Algorithm 1에서처럼 해당 방의 현재 dimension 에서 원래 dimension로 가는 scale과 해당 방의 현재 중심 위치에서 실제 공간 중심으로 가는 translation을 계산합니다. 만약 계산된 scale이 $I$ 가 아니거나 translate이 $0$ 아니라면 복원 단계가 진행됩니다. 
4. 복원(Restore) 단계에서는 위에서 구한 scale 과 translation을 합쳐서 각 벽면이 이동해야 되는 위치를 먼저 계산합니다 (line 4 in Algorithm 2). 이후 각 벽면이 해당 위치에 도달할 때 까지 Change blindness 현상을 이용하여 사용자 시야 밖 벽면들을 조금씩 이동하게 됩니다 (Figure 3(e) and line 7 in Algorithm 2). 
5. 복원이 완료되고 이후 압축(Compression) 단계에서는 복원된 방에 이웃한 방들을 실제 공간 안쪽으로 압축합니다 (Figure 3(f) and line 5 Algorithm 3). 그 결과, 이러한 복원-압축 단계의 반복으로 실제 공간 안에서 무수히 많은 방들이 서로 연결된 가상 공간을 탐험할 수 있게 됩니다.

<p align="center">
  <img 
    width="100%"
    src="/Resources/pseudocode.png"
  >
</p>
