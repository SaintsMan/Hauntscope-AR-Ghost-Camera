package com.pavko.hauntscope.share;

import android.app.Activity;
import android.content.ClipData;
import android.content.ContentResolver;
import android.content.ContentValues;
import android.content.Intent;
import android.net.Uri;
import android.os.Build;
import android.os.Environment;
import android.provider.MediaStore;
import android.util.Log;

import androidx.core.content.FileProvider;

import java.io.File;
import java.io.FileInputStream;
import java.io.InputStream;
import java.io.OutputStream;

/**
 * Photo sharing for Hauntscope. Unity calls it through JNI (AndroidShareService): the system share sheet with a
 * content:// link to one photo, and a copy into the gallery under Pictures/Hauntscope.
 */
public final class ShareBridge {
    private static final String TAG = "Hauntscope";
    private static final String MIME = "image/jpeg";
    private static final int BUFFER = 64 * 1024;

    private ShareBridge() {
    }

    public static void shareImage(Activity activity, String path, String text, String chooserTitle) {
        activity.runOnUiThread(() -> {
            try {
                Uri uri = FileProvider.getUriForFile(activity, activity.getPackageName() + ".hauntscope.share", new File(path));
                Intent send = new Intent(Intent.ACTION_SEND);
                send.setType(MIME);
                send.putExtra(Intent.EXTRA_STREAM, uri);
                send.putExtra(Intent.EXTRA_TEXT, text);
                // ClipData carries the read grant through the chooser to whichever app the player picks.
                send.setClipData(ClipData.newRawUri("", uri));
                send.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
                Intent chooser = Intent.createChooser(send, chooserTitle);
                chooser.addFlags(Intent.FLAG_GRANT_READ_URI_PERMISSION);
                activity.startActivity(chooser);
            } catch (Exception exception) {
                Log.w(TAG, "share failed", exception);
            }
        });
    }

    /** MediaStore without storage permissions exists from Android 10; older phones can still share. */
    public static boolean canSaveToGallery() {
        return Build.VERSION.SDK_INT >= Build.VERSION_CODES.Q;
    }

    public static boolean saveToGallery(Activity activity, String path, String displayName) {
        if (!canSaveToGallery())
            return false;

        ContentResolver resolver = activity.getContentResolver();
        ContentValues values = new ContentValues();
        values.put(MediaStore.Images.Media.DISPLAY_NAME, displayName);
        values.put(MediaStore.Images.Media.MIME_TYPE, MIME);
        values.put(MediaStore.Images.Media.RELATIVE_PATH, Environment.DIRECTORY_PICTURES + "/Hauntscope");
        values.put(MediaStore.Images.Media.IS_PENDING, 1);

        Uri uri = resolver.insert(MediaStore.Images.Media.EXTERNAL_CONTENT_URI, values);
        if (uri == null)
            return false;

        try (InputStream in = new FileInputStream(path); OutputStream out = resolver.openOutputStream(uri)) {
            if (out == null)
                throw new IllegalStateException("no output stream");

            byte[] buffer = new byte[BUFFER];
            int read;
            while ((read = in.read(buffer)) > 0)
                out.write(buffer, 0, read);
        } catch (Exception exception) {
            Log.w(TAG, "saving to the gallery failed", exception);
            resolver.delete(uri, null, null);
            return false;
        }

        values.clear();
        values.put(MediaStore.Images.Media.IS_PENDING, 0);
        resolver.update(uri, values, null, null);
        return true;
    }
}
