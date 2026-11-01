# /// script
# requires-python = ">=3.10"
# dependencies = ["mcp==1.25.0"]
# ///
"""Call the local Unity MCP using its actual MCP transport, without an app restart."""
import argparse
import asyncio
import json
from pathlib import Path

from mcp import ClientSession
from mcp.client.streamable_http import streamablehttp_client


async def run(args):
    async with streamablehttp_client("http://127.0.0.1:8080/mcp") as (read, write, _):
        async with ClientSession(read, write) as session:
            await session.initialize()
            if args.describe:
                result = await session.list_tools()
                matches = [tool.model_dump() for tool in result.tools if tool.name == args.describe]
                print(json.dumps(matches, indent=2))
                return
            if args.list_resources:
                result = await session.list_resources()
            elif args.resource:
                result = await session.read_resource(args.resource)
            elif args.tool:
                if not args.instance:
                    raise ValueError("Mutating/query tools require an explicit --instance from mcpforunity://instances")
                selected = await session.call_tool("set_active_instance", {"instance": args.instance})
                if selected.isError:
                    raise RuntimeError(str(selected))
                selection = selected.structuredContent
                if selection is None:
                    selection = next((json.loads(c.text) for c in selected.content if c.type == "text"), {})
                if not selection.get("success"):
                    raise RuntimeError("Unity instance selection failed: " + str(selection))
                payload = {"action": "execute", "code": Path(args.code_file).read_text(encoding="utf-8-sig")} if args.code_file else (
                    json.loads(Path(args.args_file).read_text(encoding="utf-8-sig")) if args.args_file else json.loads(args.args))
                result = await session.call_tool(args.tool, payload)
            else:
                result = await session.list_tools()
            data = getattr(result, "structuredContent", None)
            print(json.dumps(data, indent=2) if data is not None else result.model_dump_json(indent=2))
            if getattr(result, "isError", False) or (isinstance(data, dict) and data.get("success") is False):
                raise SystemExit(1)


if __name__ == "__main__":
    parser = argparse.ArgumentParser(description=__doc__)
    parser.add_argument("--resource")
    parser.add_argument("--list-resources", action="store_true")
    parser.add_argument("--describe")
    parser.add_argument("--instance")
    parser.add_argument("--tool")
    parser.add_argument("--args", default="{}")
    parser.add_argument("--args-file")
    parser.add_argument("--code-file")
    asyncio.run(run(parser.parse_args()))
