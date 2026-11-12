import { useEffect, useState } from 'react';
import * as faceapi from 'face-api.js';

let modelsLoadedPromise: Promise<void> | null = null;

async function loadModels() {
  const MODEL_URL = '/models';
  await Promise.all([
    faceapi.nets.tinyFaceDetector.loadFromUri(MODEL_URL),
    faceapi.nets.faceLandmark68Net.loadFromUri(MODEL_URL),
    faceapi.nets.faceRecognitionNet.loadFromUri(MODEL_URL),
  ]);
}

// يحمّل النماذج مرة واحدة فقط لكل الجلسة (مشتركة بين كل مكونات الالتقاط)
export function useFaceApiModels() {
  const [isReady, setIsReady] = useState(false);
  const [error, setError] = useState<Error | null>(null);

  useEffect(() => {
    if (!modelsLoadedPromise) {
      modelsLoadedPromise = loadModels();
    }
    modelsLoadedPromise
      .then(() => setIsReady(true))
      .catch((err) => setError(err instanceof Error ? err : new Error('Failed to load face models')));
  }, []);

  return { isReady, error };
}