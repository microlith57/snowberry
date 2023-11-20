local sb_loader = require("#Snowberry.Editor.LoennInterop.LoennShims")

local bit = {}

bit.lshift = sb_loader.lshift

return bit