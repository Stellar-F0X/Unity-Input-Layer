# Unity Input Layer System

Unity의 Input System을 레이어 기반으로 관리할 수 있는 확장 시스템입니다. <br>
스택 구조를 통해 UI, 게임플레이, 메뉴 등 다양한 컨텍스트에서 입력 우선순위를 자동으로 관리합니다.

## 주요 기능

- **스택 기반 입력 관리**: 레이어를 스택에 푸시/팝하여 입력 컨텍스트를 자동 전환
- **우선순위 자동 처리**: 최상단 레이어만 입력을 받아 충돌 방지
- **중앙화된 제어**: InputManager를 통한 전역 입력 흐름 관리
- **컴포넌트 기반**: InputReceiver로 입력 로직을 컴포넌트화

## 요구사항

- Unity 2019.1 이상
- Unity Input System 패키지

## 설치

### Package Manager를 통한 설치

1. Unity 에디터에서 `Window > Package Manager` 열기
2. `+` 버튼 클릭 → `Add package from git URL...` 선택
3. 다음 URL 입력:

```
https://github.com/Stellar-F0X/Unity-Input-Layer.git
```

### manifest.json을 통한 설치

`Packages/manifest.json` 파일에 다음 줄을 추가:

```json
{
  "dependencies": {
    "com.stellarfox.input-layer": "https://github.com/Stellar-F0X/Unity-Input-Layer.git"
  }
}
```

## 빠른 시작

### 1. Input Action Asset 설정

Project Settings > Input System Package에서 Input-wide로 Input Action Asset을 설정합니다.

### 2. InputManager 설정

씬에 InputManager가 자동으로 생성되거나, 직접 GameObject에 추가합니다.

추가된 InputManager 컴포넌트의 Inspector에서 Root로 삼을 레이어를 설정합니다.

```csharp
[SerializeField] private InputLayerName _rootLayer; // "Player" 등
```

**주의 Root 레이어는 런타임 중에 없앨 수 없습니다.**

### 3. 기본 사용법

```csharp
using InputLayer.Runtime;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    [SerializeField] private InputLayerController controller;

    private void Start()
    {
        controller.PushInputLayer("UI"); // UI 레이어 추가
    }

    private void OnMenuClosed()
    {
        controller.PopInputLayer(); // UI 레이어 제거 (아래 있는 레이어로 자동 복귀)
    }
}
```

## 핵심 컴포넌트

### InputLayerController

레이어 스택을 조작하고 이벤트를 구독할 수 있는 컴포넌트입니다. 
GameObject에 추가하여 사용합니다.

**메서드**

```csharp
[SerializeField] private InputLayerController controller;

// 레이어 푸시
controller.PushInputLayer("UI");

// 레이어 팝
controller.PopInputLayer();

// 루트 제외 모든 레이어 제거
controller.PopAllInputLayersExpectRoot();

// 현재 최상단 레이어 확인
InputLayer currentLayer = controller.peekInputLayer;
```

**이벤트**

```csharp
public class SomeClass : MonoBehaviour
{
    [SerializeField] private InputLayerController controller;

    private void Start()
    {
        controller.onPushedInputLayer += OnLayerPushed;
        controller.onPoppedInputLayer += OnLayerPopped;
    }

    private void OnLayerPushed(InputLayer layer)
    {
        Debug.Log($"레이어 추가됨: {layer.mapName}");
    }

    private void OnLayerPopped(InputLayer layer)
    {
        Debug.Log($"레이어 제거됨: {layer.mapName}");
    }

    private void OnDestroy()
    {
        controller.onPushedInputLayer -= OnLayerPushed;
        controller.onPoppedInputLayer -= OnLayerPopped;
    }
}
```

### InputManager

입력 레이어 스택을 관리하는 싱글톤 매니저입니다. 일반적으로 **InputLayerController**를 통해 간접적으로 사용하는 것을 권장합니다.

**직접 접근 (고급)**

```csharp
// 현재 입력이 차단되어 있는지 확인 (읽기 전용)
bool isBlocked = Singleton<InputManager>.Instance.inputBlock;

// 레이어 스택 변경 차단 설정
Singleton<InputManager>.Instance.layerStackBlock = true;  // 차단
Singleton<InputManager>.Instance.layerStackBlock = false; // 허용

// 입력 활성화/비활성화
Singleton<InputManager>.Instance.EnableControls(false); // 입력 비활성화
Singleton<InputManager>.Instance.EnableControls(true);  // 입력 활성화
```

### InputReceiver

특정 레이어의 입력을 받는 컴포넌트입니다. GameObject에 추가하고 Inspector에서 레이어를 지정합니다.

**설정**

```csharp
public class PlayerController : MonoBehaviour
{
    [SerializeField] private InputReceiver inputReceiver; // Inspector에서 레이어 지정 필요
}
```

**입력 읽기 메서드**

```csharp
private void Update()
{
    // 버튼이 이번 프레임에 눌렸는지
    if (inputReceiver.ReadButtonDown("Jump"))
    {
        Jump();
    }

    // 버튼이 현재 눌려있는지
    if (inputReceiver.ReadButton("Fire"))
    {
        Shoot();
    }

    // 버튼이 이번 프레임에 떼어졌는지
    if (inputReceiver.ReadButtonUp("Aim"))
    {
        StopAiming();
    }

    // 값 읽기 (Vector2, float 등)
    if (inputReceiver.ReadInput("Move", out Vector2 movement))
    {
        Move(movement);
    }
}
```

**콜백 등록**

```csharp
private void Start()
{
    // 콜백 등록 - 레이어가 활성화되지 않아도 콜백은 동작합니다
    _inputReceiver.RegisterInputAction("Jump", InputCallback.Performed, OnJumpPerformed);
}

private void OnJumpPerformed(InputAction.CallbackContext context)
{
    Debug.Log("Jump performed!");
}

private void OnDestroy()
{
    // 콜백 제거 필수
    _inputReceiver.UnregisterInputAction("Jump", InputCallback.Performed);
}
```

**주의사항**

- `ReadInput`, `ReadButton`, `ReadButtonDown`, `ReadButtonUp`은 해당 InputReceiver의 레이어가 **최상단 레이어가 아니면** 항상 false를 반환합니다
- 콜백(`RegisterInputAction`)은 레이어 활성화 여부와 무관하게 동작합니다
- InputReceiver는 지정된 레이어의 Input Action에만 접근 가능합니다

### InputLayerName

인스펙터에서 Input Action Map을 선택할 수 있는 직렬화 가능한 타입입니다.

```csharp
[SerializeField] 
private InputLayerName _playerLayer;

private void Start()
{
    Debug.Log(_playerLayer.name); // "Player"
    Debug.Log(_playerLayer.id);   // Guid
}
```

### InputCallback

입력 콜백 타입을 지정하는 플래그 열거형입니다.

```csharp
// 단일 콜백
InputCallback.Started
InputCallback.Canceled
InputCallback.Performed

// 복합 콜백
InputCallback.All // Started | Canceled | Performed
InputCallback.Started | InputCallback.Performed
```

## 사용 예시

### UI 메뉴 시스템

```csharp
public class PauseMenu : MonoBehaviour
{
    public InputLayerController controller;
    
    private void OnEnable()
    {
        controller.PushInputLayer("UI"); // 메뉴가 열리면 UI 레이어 활성화
    }

    private void OnDisable()
    {   
        controller.PopInputLayer(); // 메뉴가 닫히면 이전 레이어로 복귀
    }
}
```

### 대화 시스템

```csharp
public class DialogueSystem : MonoBehaviour
{
    public InputReceiver dialogueInput;
    public InputLayerController controller;

    private void StartDialogue()
    {
        controller.PushInputLayer("Dialogue"); // 대화 시작시, Dialogue 레이어만 활성화
        
        dialogueInput.RegisterInputAction("Submit", InputCallback.Performed, OnDialogueAdvance);
    }

    private void EndDialogue()
    {
        dialogueInput.UnregisterInputAction("Submit", InputCallback.Performed);
        
        controller.PopInputLayer(); // 대화 종료시, Dialogue 레이어를 제거하고 기존 레이어 활성화.
    }

    private void OnDialogueAdvance(InputAction.CallbackContext ctx)
    {
        // 다음 대화로 진행
    }
}
```

### 인벤토리 시스템

```csharp
public class InventoryUI : MonoBehaviour
{
    public InputLayerController controller;
    
    private void Open()
    {
        controller.PushInputLayer("Inventory"); // 플레이어 이동 입력이 자동으로 차단됨
    }

    private void Close()
    {
        controller.PopInputLayer(); // 플레이어 이동 입력 재개
    }
}
```

## 디버그 모드

InputManager의 인스펙터에서 Debug 옵션을 활성화하면 게임 화면 좌측 상단에 현재 레이어 스택이 표시됩니다.

- **노란색**: Root 레이어
- **초록색**: 현재 활성 레이어 (최상단)
- **흰색**: 비활성 레이어

## 아키텍처

### 레이어 스택 구조

```
┌─────────────────┐
│   UI Layer      │ ← 현재 활성 (입력 받음)
├─────────────────┤
│  Player Layer   │ ← 비활성
├─────────────────┤
│  Root Layer     │ ← 절대 제거 불가
└─────────────────┘
```

### 입력 우선순위

InputReceiver의 입력 읽기 메서드(`ReadInput`, `ReadButton` 등)는 다음 우선순위에 따라 동작합니다:

1. **입력 차단 확인**: `inputBlock`이 true면 모든 입력 차단
2. **레이어 매칭 확인**: InputReceiver의 레이어가 현재 최상단 레이어(`peekInputLayer`)와 일치하지 않으면 입력 무시
3. **입력 처리**: 최상단 레이어의 InputReceiver만 입력을 읽을 수 있음

**중요**: 콜백(`RegisterInputAction`)은 이 우선순위 규칙을 따르지 않고, 해당 Input Action이 활성화되어 있으면 항상 호출됩니다.

## 라이선스

MIT License

## 기여

이슈와 풀 리퀘스트를 환영합니다!