class_name PaymentService
extends RefCounted
## Store purchases of coin packs. Every purchase in the game goes through purchase(); connecting a real store means
## replacing only its body with the store's purchase call (on_paid after the store confirms payment, nothing on cancel
## or failure). Until then the purchase completes at once.

static func purchase(pack: Dictionary, on_paid: Callable) -> void:
	on_paid.call(pack)
