# Space Redirection for Infinite room
가상 현실에서의 탐험 기법은 여러 가지로 갈래로 나뉘어서 연구되고 있고 그 중 하나가 사용자의 실제 보행이 있습니다. 그러나, 가상과 실제 공간 차이로 인해 단순한 보행으로는 안전상 위험할 수 있고 사용자가 가상 현실에 몰입할 수 없다는 큰 단점이 존재합니다. 따라서, 사용자가 최대한 실제 공간과 충돌이 일어나지 않도록 가상 현실에서의 사용자의 보행을 미세하게 조정하는 방식 등이 연구되고 있습니다.

이러한 맥락에서 출발하여, 본 프로젝트에서는 무수히 많은 방들로 구성된 가상 실내 환경을 change blindness 현상을 이용한 벽 이동을 통해 실제 환경에서의 충돌 없이 사용자가 걸어서 탐험하는 일반화된 알고리즘을 제시하였습니다. 구체적으로 해당 기법은 사용자가 보지 않고 있는 동안 방 안의 벽면을 이동시켜 방을 scale하고 translation 하여, 현재 사용자를 포함하고 있는 가상의 방이 유효한 실제 공간 내에 항상 존재하도록 만듭니다. 따라서, 기존 방법들과 다르게 이론상 실제 공간의 충돌이 전혀 발생하지 않는다는 장점을 가집니다.

또한, 해당 프로젝트에서는 제안한 방법을 뒷받침 하기 위한 실험용 애플리케이션을 제작하였습니다. 크게 두 가지 실험을 진행하였고, 첫번째 실험은 시야 밖의 벽면이 어느 정도까지 이동해야 사용자가 이를 인지하는지 확인하는 detection threshold 실험이고, 두번째 실험은 제안한 방법과 기존의 다른 탐험 기법(Telport, Steer-to-Center)을 이용했을 때 사용자가 느끼는 사용성, 현존감 그리고 몰입감의 차이가 있는지를 확인하는 실험입니다.

# Requirements
Unity 2019.4
Unity Hub 2.4+

# How to Use
1. Clone 한 뒤에, Unity Hub를 통해서 Clone된 디렉토리를 추가하여 Unity 프로젝트로 실행합니다.
2. Asset/Scene 에서 원하는 Scene 선택하여 진입한 뒤에, Play 버튼을 눌러서 실행합니다.
* Asset/Scene 에는 차례대로 첫번째 선행 실험(VR 적응 훈련), 첫번째 실험, 두번째 선행 실험(VR 내 상호작용이나 Teleport 기능 훈련), 두번째 실험이 준비되어 있습니다. 

# Structure
구현 구조는 다음과 같습니다.

# Algorithm
본 연구에서는 일반적인 가상 실내 환경에서 사용자가 임의의 방에 위치해 있을 때 시야 밖 방의 벽 이동을 통한 공간 변형 RDW을 제안한다. 실내 공간은 방들과 이들을 이어주는 복도로 구성되어 있으며, 복도 또한 하나의 방으로 본다면 실내 공간은 무수히 많은 방들의 집합이라고 할 수 있다. 그리고 주어진 넓이 안에서 간단하면서도 효율적으로 실내 공간을 구성하기 위해 대부분의 방들은 직사각형 형태를 이루게 된다. 따라서 우리는 가상 실내 공간과 실제 공간을 Figure \ref{fig:env}와 같이 가정할 수 있다. 실제 공간 $R$ 은 직사각형이고 가상 공간 $V$ 은 $n$ 개의 방으로 구성되고 각 방은 실제 공간 안에 완전히 포함될 수 있는 크기의 직사각형이다. 그리고 각 방은 인접한 다른 방과 문으로 연결될 수 있다. 

이런 가정 하에, 제안하는 알고리즘은 다음과 같다. Figure \ref{fig:algo_step}(a), \ref{fig:algo_step}(b), and line \ref{alg:step1} in Algorithm \ref{alg:nav}에 나타낸 것처럼, 해당 방법은 우선 사용자가 처음 시작할 방을 실제 공간 중심으로 이동시키고 해당 방에 이웃한 방들을 실제 공간 안쪽으로 압축하여 초기화한다.

이후에 이 방법은 사용자가 새로운 방에 방문할 때마다 복원 후 압축 단계를 진행시킨다 (Figure \ref{fig:algo_step}(c) and line \ref{alg:step2} in Algorithm \ref{alg:nav}). 먼저 Figure \ref{fig:algo_step}(d) 와 line \ref{alg:step3-1},\ref{alg:step3-2} in Algorithm \ref{alg:nav}에서처럼 해당 방의 현재 dimension 에서 원래 dimension로 가는 scale과 해당 방의 현재 중심 위치에서 실제 공간 중심으로 가는 translation을 계산한다. 만약 계산된 scale이 $\mathbf{I}$ 가 아니거나 translate이 $\mathbf{0}$ 아니라면 복원 단계가 진행된다. 복원(Restore) 단계에서는 위에서 구한 scale 과 translation을 합쳐서 각 벽면이 이동해야 되는 위치를 먼저 계산한다 (line \ref{alg:step4} Algorithm \ref{alg:restore}). 이후 각 벽면이 해당 위치에 도달할 때 까지 Change blindness 현상을 이용하여 사용자 시야 밖 벽면들을 조금씩 이동하게 된다 (Figure \ref{fig:algo_step}(e) and line \ref{alg:step5} Algorithm \ref{alg:restore}). 복원이 완료되고 이후 압축(Compression) 단계에서는 복원된 방에 이웃한 방들을 실제 공간 안쪽으로 압축한다 (Figure \ref{fig:algo_step}(f) and line \ref{alg:step6} Algorithm \ref{alg:compression}). 그 결과, 이러한 복원-압축 단계의 반복으로 실제 공간 안에서 무수히 많은 방들이 서로 연결된 가상 공간을 탐험할 수 있게 된다.
