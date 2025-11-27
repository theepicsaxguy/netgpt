"""Package marker for netgpt-docs-service.app

This module applies minimal pre-configuration (threading/env vars) as soon
as the package is imported so native libraries like onnxruntime see the
desired environment and avoid trying to set CPU affinity in environments
where that may fail.
"""

# Import preconfig as early as possible when the package is imported.
try:
	from .preconfig import configure_from_env
	# apply defaults
	configure_from_env()
except Exception:
	# best-effort: if preconfig cannot be imported, continue without failing
	pass
