package com.spidervr.mp;

import android.app.Activity;
import android.graphics.Bitmap;
import com.google.mediapipe.framework.image.BitmapImageBuilder;
import com.google.mediapipe.framework.image.MPImage;
import com.google.mediapipe.tasks.components.containers.Category;
import com.google.mediapipe.tasks.components.containers.NormalizedLandmark;
import com.google.mediapipe.tasks.core.BaseOptions;
import com.google.mediapipe.tasks.vision.core.RunningMode;
import com.google.mediapipe.tasks.vision.handlandmarker.HandLandmarker;
import com.google.mediapipe.tasks.vision.handlandmarker.HandLandmarkerResult;
import com.google.mediapipe.tasks.vision.poselandmarker.PoseLandmarker;
import com.google.mediapipe.tasks.vision.poselandmarker.PoseLandmarkerResult;
import java.nio.ByteBuffer;
import java.util.List;
import java.util.concurrent.ExecutorService;
import java.util.concurrent.Executors;

/** Ponte Unity -> MediaPipe Tasks (maos + corpo). Roda em thread propria; Unity so envia frames RGBA e le os resultados. */
public class MPBridge {
    private final Activity act;
    private HandLandmarker hand;
    private PoseLandmarker pose;
    private final ExecutorService ex = Executors.newSingleThreadExecutor();
    private volatile boolean busy = false, ready = false;
    private volatile String error = "";
    private volatile float[] handOut = new float[130];
    private volatile float[] poseOut = new float[33 * 4];
    private volatile long stamp = 0;
    private int frameNo = 0;

    public MPBridge(Activity a) { act = a; }

    public void init() {
        ex.execute(new Runnable() {
            public void run() {
                try {
                    hand = HandLandmarker.createFromOptions(act, HandLandmarker.HandLandmarkerOptions.builder()
                        .setBaseOptions(BaseOptions.builder().setModelAssetPath("hand_landmarker.task").build())
                        .setNumHands(2).setRunningMode(RunningMode.IMAGE).build());
                    pose = PoseLandmarker.createFromOptions(act, PoseLandmarker.PoseLandmarkerOptions.builder()
                        .setBaseOptions(BaseOptions.builder().setModelAssetPath("pose_landmarker_lite.task").build())
                        .setRunningMode(RunningMode.IMAGE).build());
                    ready = true;
                } catch (Throwable t) { error = t.toString(); }
            }
        });
    }

    public boolean isReady() { return ready; }
    public String getError() { return error; }
    public float[] hands() { return handOut; }
    public float[] poseLm() { return poseOut; }
    public long getStamp() { return stamp; }

    /** rgba: w*h*4 bytes, linha de cima primeiro. Ignora o frame se ainda estiver processando o anterior. */
    public void submit(final byte[] rgba, final int w, final int h) {
        if (!ready || busy) return;
        busy = true;
        ex.execute(new Runnable() {
            public void run() {
                Bitmap bmp = null;
                try {
                    bmp = Bitmap.createBitmap(w, h, Bitmap.Config.ARGB_8888);
                    bmp.copyPixelsFromBuffer(ByteBuffer.wrap(rgba));
                    MPImage img = new BitmapImageBuilder(bmp).build();
                    fillHands(hand.detect(img));
                    if ((frameNo++ & 1) == 0) fillPose(pose.detect(img));   // corpo a cada 2 frames: poupa bateria
                    stamp = System.nanoTime();
                } catch (Throwable t) { error = t.toString(); }
                finally { if (bmp != null) bmp.recycle(); busy = false; }
            }
        });
    }

    private void fillHands(HandLandmarkerResult r) {
        float[] o = new float[130];
        List<List<NormalizedLandmark>> lms = r.landmarks();
        List<List<Category>> hd = r.handedness();
        for (int i = 0; i < lms.size() && i < 2; i++) {
            int b = i * 65;
            o[b] = 1f;
            o[b + 1] = "Right".equals(hd.get(i).get(0).categoryName()) ? 1f : 0f;
            List<NormalizedLandmark> l = lms.get(i);
            for (int j = 0; j < 21 && j < l.size(); j++) {
                o[b + 2 + j * 3] = l.get(j).x();
                o[b + 3 + j * 3] = l.get(j).y();
                o[b + 4 + j * 3] = l.get(j).z();
            }
        }
        handOut = o;
    }

    private void fillPose(PoseLandmarkerResult r) {
        float[] o = new float[33 * 4];
        if (!r.landmarks().isEmpty()) {
            List<NormalizedLandmark> l = r.landmarks().get(0);
            for (int j = 0; j < 33 && j < l.size(); j++) {
                o[j * 4] = l.get(j).x(); o[j * 4 + 1] = l.get(j).y(); o[j * 4 + 2] = l.get(j).z(); o[j * 4 + 3] = vis(l.get(j));
            }
        }
        poseOut = o;
    }

    // visibility() muda de tipo entre versoes (float / Optional<Float>): le por reflexao para nao quebrar a compilacao.
    private static float vis(NormalizedLandmark l) {
        try {
            Object o = NormalizedLandmark.class.getMethod("visibility").invoke(l);
            if (o instanceof java.util.Optional) { Object v = ((java.util.Optional<?>) o).orElse(null); return v == null ? 0f : ((Number) v).floatValue(); }
            if (o instanceof Number) return ((Number) o).floatValue();
        } catch (Throwable t) { }
        return 1f;
    }
}
