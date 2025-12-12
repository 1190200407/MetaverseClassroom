using Eitan.Sherpa.Onnx.Unity.Mono.Components;
using Eitan.Sherpa.Onnx.Unity.Mono.Inputs;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// code by Eitan
/// Streaming speech-to-text demo with clear UI feedback.
/// 实时语音识别示例，提供清晰的加载与录音反馈。
/// </summary>
public sealed class RealtimeSpeechRecognitionUI : MonoBehaviour
{
    [Header("Sherpa Components")]
    [SerializeField] private SpeechRecognizerComponent recognizer;
    [SerializeField] private SherpaMicrophoneInput microphone;

    [Header("UI")]
    [SerializeField] private Text transcriptText;

    [Header("Defaults")]
    [SerializeField] private string defaultModelID = "sherpa-onnx-streaming-zipformer-bilingual-zh-en-2023-02-20";

    private bool moduleRequested;
    private bool modelReady;

    private void Awake()
    {
        modelReady = false;
        moduleRequested = false;
        if (recognizer != null)
        {
            recognizer.TranscriptionReadyEvent.AddListener(HandleTranscriptReady);
            recognizer.InitializationStateChangedEvent.AddListener(HandleModelReady);
            if (microphone != null)
            {
                recognizer.BindInput(microphone);
            }
        }
    }

    private void OnDestroy()
    {
        if (recognizer != null)
        {
            recognizer.TranscriptionReadyEvent.RemoveListener(HandleTranscriptReady);
            recognizer.InitializationStateChangedEvent.RemoveListener(HandleModelReady);
        }
    }

    private void Start()
    {
        if (recognizer != null && !moduleRequested)
        {
            var modelId = defaultModelID;
            recognizer.ModelId = modelId.Trim();
            if (recognizer.TryLoadModule())
            {
                moduleRequested = true;
                modelReady = false;
                transcriptText.text = "Loading model...";
            }
            else
            {
                transcriptText.text = "Model already loading or missing configuration.";
            }
        }
    }

    private void HandleModelReady(bool ready)
    {
        modelReady = ready && moduleRequested;
        
        if (modelReady)
        {
            transcriptText.text = "";
        }
    }

    private void HandleTranscriptReady(string transcript)
    {
        if (string.IsNullOrWhiteSpace(transcript))
        {
            return;
        }

        // 实时更新识别结果 / Update transcript in real time
        if (transcriptText != null)
        {
            transcriptText.text = transcript;
        }
    }
}
