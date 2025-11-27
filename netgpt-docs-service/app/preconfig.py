"""Pre-configuration to set environment variables that control native library
threading and affinity behavior. Import this before any heavy native libraries
like onnxruntime are loaded to avoid pthread_setaffinity_np errors.

This is intentionally conservative; tune values via environment variables.
"""
from __future__ import annotations
import os

# If these variables are already set by the environment, keep them. Otherwise
# set conservative defaults to avoid aggressive thread affinity/parallelism which
# can trigger `pthread_setaffinity_np` errors in some container/VM setups.
os.environ.setdefault("OMP_NUM_THREADS", os.environ.get("OMP_NUM_THREADS", "1"))
os.environ.setdefault("MKL_NUM_THREADS", os.environ.get("MKL_NUM_THREADS", "1"))
os.environ.setdefault("OPENBLAS_NUM_THREADS", os.environ.get("OPENBLAS_NUM_THREADS", "1"))
os.environ.setdefault("NUMEXPR_NUM_THREADS", os.environ.get("NUMEXPR_NUM_THREADS", "1"))
# Prevent OpenMP from binding threads to specific CPUs (may help in some runtimes)
os.environ.setdefault("OMP_PROC_BIND", os.environ.get("OMP_PROC_BIND", "false"))
# Additional vars for Intel/OpenMP runtimes
os.environ.setdefault("KMP_AFFINITY", os.environ.get("KMP_AFFINITY", "disabled"))
os.environ.setdefault("KMP_BLOCKTIME", os.environ.get("KMP_BLOCKTIME", "0"))

def configure_from_env():
    """No-op function to make it explicit clients can call this if desired."""
    return {
        "OMP_NUM_THREADS": os.environ["OMP_NUM_THREADS"],
        "MKL_NUM_THREADS": os.environ["MKL_NUM_THREADS"],
    }
