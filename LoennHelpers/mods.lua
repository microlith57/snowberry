local sb_loader = require("#Snowberry.LoennPluginLoader")

local mods = {}

-- wrap on the lua-side so mod can be autofilled with nil
function mods.requireFromPlugin(name, mod)
    return sb_loader.RequireFromMods(name, mod or "")
end

return mods