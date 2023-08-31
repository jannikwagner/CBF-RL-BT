from plotting import get_scalar_dataframe_from_tb_files

run_id = "env5.wcbf.fixedbridge.safeplace"
file_path = f"results/{run_id}"

df = get_scalar_dataframe_from_tb_files([file_path])
print(df)
