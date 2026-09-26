package com.pavko.hauntscope.share;

import androidx.core.content.FileProvider;

/**
 * Hauntscope's own provider class, so its manifest entry never clashes with another library that declares
 * androidx's FileProvider directly.
 */
public final class ShareFileProvider extends FileProvider {
}
