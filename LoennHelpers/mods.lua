local sb_loader = require("#Snowberry.LoennPluginLoader")

local mods = {}

-- wrap on the lua-side so mod can be autofilled with nil
function mods.requireFromPlugin(name, mod)
    local lib = sb_loader.RequireFromMods(name, mod or "")
    return lib
end

function mods.getModSettings()
    return {}
end

function mods.findLoadedMod(modName)
    local info = sb_loader.FindLoadedMod(modName)
    return info
end

return mods