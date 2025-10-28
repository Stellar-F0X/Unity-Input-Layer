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
    private void Start()
    {
        InputManager.Instance.PushInputLayer("UI"); // UI 레이어 추가
    }

    private void OnMenuClosed()
    {
        InputManager.Instance.PopInputLayer(); // UI 레이어 제거 (아래 있는 레이어로 자동 복귀)
    }
}
```

## 핵심 컴포넌트

### InputManager

입력 레이어 스택을 관리하는 싱글톤 매니저입니다.


**일반 메서드들**

```csharp
Singleton<InputManager>.Instance.PushInputLayer("UI"); // 레이어 푸시

Singleton<InputManager>.Instance.PopInputLayer(); // 레이어 팝

Singleton<InputManager>.Instance.PopAllInputLayerWithoutRoot(); // 루트 제외 모든 레이어 제거
```

**정적 필드들**

```csharp
InputLayer currentLayer = InputManager.PeekInputLayer; // 현재 최상단 레이어 확인

InputManager.InputBlock = true; // 전역 입력 차단

InputManager.LayerStackBlock = true; // 레이어 스택 변경 차단
```

**이벤트**
```csharp
InputManager.Instance.onPushedInputLayer += OnLayerPushed;
InputManager.Instance.onPoppedInputLayer += OnLayerPopped;
```

### InputReceiver

특정 레이어의 입력을 받는 컴포넌트입니다.

```csharp
using InputLayer.Runtime;
using UnityEngine;
using UnityEngine.InputSystem;

public class PlayerController : MonoBehaviour
{
    private InputReceiver _inputReceiver;

    private void Awake()
    {
        _inputReceiver = GetComponent<InputReceiver>();
    }

    private void Update()
    {
        // 버튼 입력
        if (_inputReceiver.ReadButtonDown("Jump"))
        {
            Jump();
        }

        // 벡터 입력
        if (_inputReceiver.ReadInput("Move", out Vector2 movement))
        {
            Move(movement);
        }
    }
}
```

**콜백 등록**
```csharp
private void Start()
{
    _inputReceiver.RegisterInputAction("Jump", InputCallback.Performed, OnJumpPerformed);
}

private void OnJumpPerformed(InputAction.CallbackContext context)
{
    Debug.Log("Jump performed!");
}

private void OnDestroy()
{
    _inputReceiver.UnregisterInputAction("Jump", InputCallback.Performed);
}
```

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
    private void OnEnable()
    {
        InputManager.Instance.PushInputLayer("UI"); // 메뉴가 열리면 UI 레이어 활성화
    }

    private void OnDisable()
    {   
        InputManager.Instance.PopInputLayer(); // 메뉴가 닫히면 이전 레이어로 복귀
    }
}
```

### 대화 시스템

```csharp
public class DialogueSystem : MonoBehaviour
{
    private InputReceiver _dialogueInput;

    private void StartDialogue()
    {
        InputManager.Instance.PushInputLayer("Dialogue"); // 대화 중에는 Dialogue 레이어만 활성화
        
        _dialogueInput.RegisterInputAction("Submit", InputCallback.Performed, OnDialogueAdvance);
    }

    private void EndDialogue()
    {
        _dialogueInput.UnregisterInputAction("Submit", InputCallback.Performed);

        InputManager.Instance.PopInputLayer();
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
    private void Open()
    {
        InputManager.Instance.PushInputLayer("Inventory"); // 플레이어 이동 입력이 자동으로 차단됨
    }

    private void Close()
    {
        InputManager.Instance.PopInputLayer(); // 플레이어 이동 입력 재개
    }
}
```

## 디버그 모드

InputManager의 Inspector에서 Debug 옵션을 활성화하면 게임 화면 좌측 상단에 현재 레이어 스택이 표시됩니다.

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

1. `InputManager.InputBlock`이 true면 모든 입력 차단
2. 최상단 레이어가 아닌 InputReceiver는 입력 무시
3. 최상단 레이어의 InputReceiver만 입력 처리

## API 레퍼런스

### InputManager

| 메서드 | 설명 |
|--------|------|
| `PushInputLayer(string)` | 레이어를 스택에 추가하고 활성화 |
| `PopInputLayer()` | 최상단 레이어 제거 (Root 제외) |
| `PopAllInputLayerWithoutRoot()` | Root를 제외한 모든 레이어 제거 |
| `EnableControls(bool)` | 현재 레이어의 입력 활성화/비활성화 |

| 프로퍼티 | 설명 |
|----------|------|
| `PeekInputLayer` | 현재 최상단 레이어 |
| `InputBlock` | 전역 입력 차단 플래그 |
| `LayerStackBlock` | 레이어 변경 차단 플래그 |

### InputReceiver

| 메서드 | 설명 |
|--------|------|
| `ReadButton(string)` | 버튼이 눌려있는지 확인 |
| `ReadButtonDown(string)` | 버튼이 이번 프레임에 눌렸는지 확인 |
| `ReadButtonUp(string)` | 버튼이 이번 프레임에 떼어졌는지 확인 |
| `ReadInput<T>(string, out T)` | 입력 값 읽기 (Vector2, float 등) |
| `RegisterInputAction(...)` | 입력 콜백 등록 |
| `UnregisterInputAction(...)` | 입력 콜백 해제 |

## 라이선스

MIT License

## 기여

이슈와 풀 리퀘스트를 환영합니다!